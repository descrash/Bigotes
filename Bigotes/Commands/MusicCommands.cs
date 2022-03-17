using Bigotes.Clases;
using Bigotes.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        #region Propiedades
        MusicPlaylist _mpl;
        #endregion

        /// <summary>
        /// Comando para reproducir música.
        /// IMPORTANTE: Ha de estar ejecutándose el servidor LAVALINK
        /// para poder encontrar una conexión.
        /// ACTUALMENTE EN PROCESO...
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Command("play")]
        [Description("Comando para reproducir música (EN PRUEBAS).")]
        public async Task Play(CommandContext ctx, [Description("Texto de búsqueda o URL.")][RemainingText]string query)
        {
            #region Propiedades
            LavalinkLoadResult loadResult;
            bool currentlyPlaying = false;
            #endregion

            try
            {
                //throw new Exception("Opción no implementada.");

                await ctx.Channel.SendMessageAsync("`[AVISO]` ```Los-métodos-de-reproducción-de-música-se-encuentran-en-período-de-pruebas. Podría-no-funcionar-correctamente.```").ConfigureAwait(false);

                #region 1. Comprobación de que el usuario se encuentra en un canal de voz
                if (ctx.Member.VoiceState == null) throw new Exception("La solicitud de música debe hacerse desde un canal de voz.");
                #endregion

                #region 2. Conexión de Lavalink y obtención de propiedades de la Guild (servidor)
                var lava = ctx.Client.GetLavalink();

                var node = lava.ConnectedNodes.Values.First();
                var connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
                #endregion

                #region 3. Comprobar la conexión del servidor y si se encuentra reproduciendo en otro canal en el momento
                if (connection == null)
                {
                    await Join(ctx, lava, ctx.Member.VoiceState.Channel);
                    connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
                }
                else if (connection.Channel != ctx.Member.VoiceState.Channel) throw new Exception("Bigotes se encuentra cantando en otro canal en este momento.");
                #endregion

                #region 4. Comprobar si existe el objeto y está reproduciendo en este momento

                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();
                LavalinkTrack playableTrack; //Variable para la reproducción

                if (_mpl == null || _mpl.playList.Count() == 0)
                {
                    //La lista está vacía
                    _mpl = new MusicPlaylist(connection, ctx.Guild);
                }
                else
                {
                    currentlyPlaying = true;
                    //Sacamos la lista para la realización de cambios...
                    Utiles.listaPlaylists.Remove(_mpl);
                }
                #endregion

                #region 5. Añadimos lista o pista a la lista de reproducción del servidor
                if (connection == null) throw new Exception("Conexión-a-servidor-no-encontrada.");

                //Ejemplo: https://open.spotify.com/track/5FVNpPC0H4fVc4FJin5vBg?si=19fc0d926cb54884
                //Ejemplo de playlist: https://open.spotify.com/playlist/07ZkxX3cj3M5lTVwc07tcx?si=3f7355440bfc4c24

                //Comprobamos si query es una URL de Spotify
                if (query.Contains("https://open.spotify.com"))
                {
                    var querySubSTR = query.Split('/', '?');

                    if (querySubSTR[3] == "playlist")
                    {
                        //Es una playlist
                        var playlistID = querySubSTR[4];

                        #region Configuración de Spotify
                        //SpotifyAPI-NET necesita crear su propio archivo de configuración y cliente OAuth para las credenciales
                        var spotiConfig = SpotifyClientConfig.CreateDefault();

                        var request = new ClientCredentialsRequest(Utiles.ConfigJson.SpotifyClient, Util.Utiles.ConfigJson.SpotifySecretClient);
                        var response = await new OAuthClient(spotiConfig).RequestToken(request);

                        //Se comprueba si el Token ha caducado...

                        if (response.IsExpired)
                        {
                            var refreshResponse = await new OAuthClient().RequestToken(new TokenSwapTokenRequest(new Uri("http://localhost"), Utiles.ConfigJson.SpotifyRefreshToken));
                            Util.Utiles.spotifyClient = new SpotifyClient(refreshResponse.AccessToken);
                        }
                        else
                        {
                            Util.Utiles.spotifyClient = new SpotifyClient(spotiConfig.WithToken(response.AccessToken));
                        }
                        #endregion

                        var playlist = await Utiles.spotifyClient.Playlists.Get(playlistID);
                        //Añadimos la playlist a la lista
                        foreach (var _track in playlist.Tracks.Items)
                        {
                            var fullTrack = (SpotifyAPI.Web.FullTrack)_track.Track;
                            _mpl.playList.Add(new MusicTrack(fullTrack.Name, fullTrack.Artists.First().Name, fullTrack.PreviewUrl));
                        }
                        await ctx.Channel.SendMessageAsync($"```Añadidas-{playlist.Tracks.Items.Count}-canciones-a-la-cola-de-reproducción.```");
                    }
                    else
                    {
                        //Es una única canción
                        var trackID = querySubSTR[4];
                        var track = await Utiles.spotifyClient.Tracks.Get(trackID);
                        _mpl.playList.Add(new MusicTrack(track.Name, track.Artists.First().Name, track.PreviewUrl));
                        //Añadimos la canción a la lista
                        await ctx.Channel.SendMessageAsync($"```Añadida-{track.Name}-de-{track.Artists.First().Name}-a-la-cola-de-reproducción.```");
                    }
                }
                else
                {
                    loadResult = await node.Rest.GetTracksAsync(query);      
                    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    {
                        throw new Exception($"La búsqueda no ha encontrado ninguna pista con estas características: {query}.");
                    }

                    playableTrack = loadResult.Tracks.First();
                    _mpl.playList.Add(new MusicTrack(playableTrack.Title, playableTrack.Author, playableTrack.Uri.ToString()));

                    await ctx.Channel.SendMessageAsync($"```Añadida-{playableTrack.Title}-de-{playableTrack.Author}-a-la-cola-de-reproducción.```");
                }
                #endregion

                #region 6. En caso de no haber nada reproduciendo, se procederá a su reproducción
                while (!currentlyPlaying && _mpl.playList.Count > 0)
                {
                    var firstTrack = _mpl.playList.First();
                    loadResult = await node.Rest.GetTracksAsync($"{firstTrack.Name} {firstTrack.Author}");
                    
                    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    {
                        await Error.MostrarError(ctx.Channel, $"Error al reproducir la pista: {firstTrack.Name} de {firstTrack.Author}.");
                        _mpl.playList.RemoveAt(0);
                    }
                    else
                    {
                        currentlyPlaying = true;
                        await _mpl.connection.PlayAsync(loadResult.Tracks.First());
                        _mpl.connection.PlaybackFinished += PistaTerminada;
                        await ctx.Channel.SendMessageAsync($"`[REPRODUCIENDO]` ```{firstTrack.Name}, de-{firstTrack.Author}```").ConfigureAwait(false);
                    }
                }

                if (currentlyPlaying)
                {
                    _mpl.textTriggerChannel = ctx.Channel;
                    Utiles.listaPlaylists.Add(_mpl);
                }
                #endregion
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Método que se triggerea cuando una canción termina de reproducirse,
        /// iniciando la siguiente en la lista o borrando ésta.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task PistaTerminada(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            try
            {
                _mpl = Utiles.listaPlaylists.Where(x => x.guild == sender.Guild).FirstOrDefault();
                if (_mpl.playList.Count == 1)
                {
                    Utiles.listaPlaylists.Remove(_mpl);
                    await _mpl.textTriggerChannel.SendMessageAsync($"```Reproducción-terminada.```").ConfigureAwait(false);
                    await _mpl.connection.DisconnectAsync().ConfigureAwait(false);
                }
                else
                {
                    _mpl.playList.Remove(_mpl.playList.First());
                    var firstTrack = _mpl.playList.First();
                    var loadResult = await _mpl.connection.Node.Rest.GetTracksAsync($"{firstTrack.Name} {firstTrack.Author}");    
                    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    {
                        throw new Exception( $"Error al reproducir la pista: {firstTrack.Name} de {firstTrack.Author}.");
                    }
                    await _mpl.connection.PlayAsync(loadResult.Tracks.First());
                    await _mpl.textTriggerChannel.SendMessageAsync($"`[REPRODUCIENDO]` ```{firstTrack.Name}, de-{firstTrack.Author}```").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(_mpl.textTriggerChannel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para pausar la reproducción actual de música
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("pause")]
        [Description("Comando para pausar la música que se está reproduciendo.")]
        public async Task Pause(CommandContext ctx)
        {
            try
            {
                #region Comprobación de que el usuario se encuentra en un canal de voz
                if (ctx.Member.VoiceState == null) throw new Exception("La solicitud de música debe hacerse desde un canal de voz.");
                #endregion

                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();

                if (_mpl == null || _mpl.estado != Utiles.PlaylistStatus.PLAYING)
                {
                    //La lista está vacía
                    throw new Exception("No estoy reproduciendo en este momento.");
                }
                else
                {
                    #region Comprobación de que el usuario está en el mismo canal de voz
                    if (_mpl.connection.Channel != ctx.Member.VoiceState.Channel) throw new Exception("Para usar este comando, se ha de estar conectado al mismo canal de voz.");
                    #endregion

                    Utiles.listaPlaylists.Remove(_mpl);
                    await _mpl.connection.PauseAsync();
                    _mpl.estado = Utiles.PlaylistStatus.PAUSE;
                    _mpl.textTriggerChannel = ctx.Channel;
                    Utiles.listaPlaylists.Add(_mpl);
                    await ctx.Channel.SendMessageAsync("```Música-pausada.```").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para pausar la reproducción actual de música
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("resume")]
        [Description("Comando para reanudar la música pausada en ese momento.")]
        public async Task Resume(CommandContext ctx)
        {
            try
            {
                #region Comprobación de que el usuario se encuentra en un canal de voz
                if (ctx.Member.VoiceState == null) throw new Exception("La solicitud de música debe hacerse desde un canal de voz.");
                #endregion

                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();

                if (_mpl == null || _mpl.estado != Utiles.PlaylistStatus.PAUSE)
                {
                    //La lista está vacía
                    throw new Exception("No tengo música pausada en este momento.");
                }
                else
                {
                    #region Comprobación de que el usuario está en el mismo canal de voz
                    if (_mpl.connection.Channel != ctx.Member.VoiceState.Channel) throw new Exception("Para usar este comando, se ha de estar conectado al mismo canal de voz.");
                    #endregion

                    Utiles.listaPlaylists.Remove(_mpl);
                    await _mpl.connection.ResumeAsync();
                    _mpl.estado = Utiles.PlaylistStatus.PLAYING;
                    _mpl.textTriggerChannel = ctx.Channel;
                    Utiles.listaPlaylists.Add(_mpl);
                    await ctx.Channel.SendMessageAsync("```Música-reanudada.```").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para parar la música
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("stop")]
        [Description("Comando para parar de reproducir la música y vaciar la lista...")]
        public async Task Stop(CommandContext ctx)
        {
            try
            {
                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).First();
                Utiles.listaPlaylists.Remove(_mpl);
                _mpl.playList = new List<MusicTrack>();
                Utiles.listaPlaylists.Add(_mpl);
                await _mpl.connection.StopAsync();
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para pasar a la siguiente canción que se está reproduciendo en ese momento
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("next")]
        [Description("Comando para pasar a la siguiente canción de la pista que se está reproduciendo.")]
        public async Task Next(CommandContext ctx)
        {
            try
            {
                #region Comprobación de que el usuario se encuentra en un canal de voz
                if (ctx.Member.VoiceState == null) throw new Exception("La solicitud de música debe hacerse desde un canal de voz.");
                #endregion

                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();

                if (_mpl == null)
                {
                    //La lista está vacía
                    throw new Exception("No estoy reproduciendo en este momento.");
                }
                else
                {
                    #region Comprobación de que el usuario está en el mismo canal de voz
                    if (_mpl.connection.Channel != ctx.Member.VoiceState.Channel) throw new Exception("Para usar este comando, se ha de estar conectado al mismo canal de voz.");
                    #endregion

                    //No hay más canciones después
                    if (_mpl.playList.Count == 1) throw new Exception("Es la última canción de la cola.");

                    //Utiles.listaPlaylists.Remove(_mpl);
                    _mpl.textTriggerChannel = ctx.Channel;
                    await _mpl.connection.StopAsync();
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para mostrar las diez primeras canciones de la cola en reproducción
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("queue")]
        [Description("Comando para mostrar las diez primeras canciones de la cola en reproducción.")]
        public async Task Queue(CommandContext ctx)
        {
            try
            {
                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();

                if (_mpl == null)
                {
                    //La lista está vacía
                    throw new Exception("No estoy reproduciendo en este momento.");
                }
                else
                {
                    StringBuilder queue = new StringBuilder();

                    for (int i = 0; i < _mpl.playList.Count() && i < 10; i++)
                    {
                        queue.Append($"**{i+1}.** {_mpl.playList[i].Name}, de {_mpl.playList[i].Author}\n");
                    }

                    if (_mpl.playList.Count() > 10)
                    {
                        queue.Append($"*... y {_mpl.playList.Count() - 10} más...*\n");
                    }

                    var embedQueue = new DiscordEmbedBuilder
                    {
                        Title = "LISTA-DE-REPRODUCCIÓN-ACTUAL",
                        Description = queue.ToString(),
                        Color = DiscordColor.DarkGreen
                    };

                    await ctx.Channel.SendMessageAsync(embed: embedQueue).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para mezclar la cola de canciones
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("shuffle")]
        [Description("Comando para mezclar la cola en reproducción.")]
        public async Task Shuffle(CommandContext ctx)
        {
            try
            {
                #region Comprobación de que el usuario se encuentra en un canal de voz
                if (ctx.Member.VoiceState == null) throw new Exception("La solicitud de música debe hacerse desde un canal de voz.");
                #endregion

                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();

                if (_mpl == null)
                {
                    //La lista está vacía
                    throw new Exception("No estoy reproduciendo en este momento.");
                }
                else
                {
                    #region Comprobación de que el usuario está en el mismo canal de voz
                    if (_mpl.connection.Channel != ctx.Member.VoiceState.Channel) throw new Exception("Para usar este comando, se ha de estar conectado al mismo canal de voz.");
                    #endregion

                    if (_mpl.playList.Count() == 1) throw new Exception("Pero si sólo hay una canción...");

                    var rand = new Random();
                    Utiles.listaPlaylists.Remove(_mpl);
                    var firstTrack = _mpl.playList.First();
                    
                    _mpl.playList = _mpl.playList.OrderBy(x => rand.Next()).ToList<MusicTrack>();
                    _mpl.playList.Remove(firstTrack); //Borrar la primera pista para no repetirla

                    Utiles.listaPlaylists.Add(_mpl);

                    await ctx.Channel.SendMessageAsync("```Lista-mezclada.```").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Comando para limpiar la lista de reproducción
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("clear")]
        [Description("Comando para limpiar la lista de reproducción.")]
        public async Task Clear(CommandContext ctx)
        {
            try
            {
                _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).First();
                Utiles.listaPlaylists.Remove(_mpl);
                _mpl.playList = new List<MusicTrack>();
                Utiles.listaPlaylists.Add(_mpl);
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx.Channel, ex.Message);
            }
        }

        /// <summary>
        /// Método para desconectarse (utilizarlo como emergencia
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        [Command("leave")]
        [Description("Comando para desconectar a Bigotes de un canal de voz en caso de que se quede atascado en el mismo.")]
        public async Task Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            try
            {
                if (lava == null) throw new Exception("Conexión-a-lavalink-no-encontrada.");
                if (!lava.ConnectedNodes.Any()) throw new Exception("Conexión-a-nodos-no-encontrada.");

                var node = lava.ConnectedNodes.Values.First();

                var conn = node.GetGuildConnection(ctx.Guild);

                if (conn == null) throw new Exception("Conexión-no-realizada.");

                await conn.DisconnectAsync();
                //await ctx.RespondAsync($"Desconectado-de-{channel.Name}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task Join(CommandContext ctx, LavalinkExtension lava, DiscordChannel channel)
        {
            try
            {
                if (lava == null) throw new Exception("Conexión a lavalink no encontrada.");

                if (!lava.ConnectedNodes.Any()) throw new Exception("No encontrados nodos conectados.");

                var node = lava.ConnectedNodes.Values.First();

                if (channel.Type != DSharpPlus.ChannelType.Voice) throw new Exception("El canal de voz no es válido.");

                await node.ConnectAsync(channel);
                //await ctx.RespondAsync($"Conectado-a-{channel.Name}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

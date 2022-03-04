using Bigotes.Clases;
using Bigotes.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando para reproducir música.
        /// IMPORTANTE: Ha de estar ejecutándose el servidor LAVALINK
        /// para poder encontrar una conexión.
        /// ACTUALMENTE EN PROCESO...
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [Command("play")]
        [Description("Comando para reproducir música (EN PROGRESO)")]
        public async Task Play(CommandContext ctx, [RemainingText]string url)
        {
            #region Propiedades
            string outputMSG = "Reproduciendo-pista...";
            #endregion

            try
            {
                //throw new Exception("Opción no implementada.");

                #region 1. Comprobación de que el usuario se encuentra en un canal de voz
                if (ctx.Member.VoiceState.Channel == null) throw new Exception("La solicitud de música debe hacerse desde un canal de voz.");
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

                #region 4. Comprobar si está reproduciendo una pista actualmente para cambiar el mensaje
                if (connection.CurrentState.CurrentTrack != null) outputMSG = "Añadiendo-pista-a-playlist...";
                #endregion

                #region 5. Buscar pista
                if (connection == null) throw new Exception("Conexión-a-servidor-no-encontrada.");
                var loadResult = await node.Rest.GetTracksAsync(url);
                //Si algo ha ido mal en la carga de Lavalink                       
                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                    //o si no puede encontrar nada
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    throw new Exception($"Error-cargando: {url}.");
                }

                connection.PlaybackFinished += PistaTerminada;
                #endregion

                #region 6. Añadir pista a cola actual
                var tracks = loadResult.Tracks;

                if (tracks.Count() == 1)
                {
                    outputMSG += $" {tracks.First().Title}-de-{tracks.First().Author}";
                }
                else if (tracks.Count() > 1)
                {
                    outputMSG = outputMSG.Replace("pista", "lista");
                }

                MusicPlaylist _mpl = Utiles.listaPlaylists.Where(x => x.guild == ctx.Guild).FirstOrDefault();

                foreach (var _track in tracks)
                {

                    if (_mpl == null || _mpl.playList.Count() == 0)
                    {
                        Utiles.listaPlaylists.Add(new MusicPlaylist {
                            guild = ctx.Guild,
                            playList = new List<LavalinkTrack> { _track },
                            estado = Utiles.PlaylistStatus.PLAYING
                        });
                        await connection.PlayAsync(_track);
                    }
                    else
                    {
                        Utiles.listaPlaylists.Remove(_mpl);
                        _mpl.playList.Add(_track);
                        Utiles.listaPlaylists.Add(_mpl);
                    }
                }
                #endregion

                await ctx.RespondAsync(outputMSG);
            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx, ex.Message);
            }
        }

        private async Task PistaTerminada(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            try
            {
                MusicPlaylist _mpl = Utiles.listaPlaylists.Where(x => x.guild == sender.Guild).First();
                if (_mpl.playList.Count == 1)
                {
                    Utiles.listaPlaylists.Remove(_mpl);
                }
                else
                {
                    Utiles.listaPlaylists.Remove(_mpl);
                    _mpl.playList.Remove(_mpl.playList.First());
                    Utiles.listaPlaylists.Add(_mpl);
                    await sender.PlayAsync(_mpl.playList.First());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Comando para pausar la reproducción actual de música
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("pausa")]
        [Description("Comando para pausar la música que se está reproduciendo en ese moment")]
        public async Task Pause(CommandContext ctx)
        {
            try
            {

            }
            catch (Exception ex)
            {
                await Error.MostrarError(ctx, ex.Message);
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

        public async Task Leave(CommandContext ctx, DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();

            try
            {
                if (lava == null) throw new Exception("Conexión-a-lavalink-no-encontrada.");
                if (!lava.ConnectedNodes.Any()) throw new Exception("Conexión-a-nodos-no-encontrada.");

                var node = lava.ConnectedNodes.Values.First();

                if (channel.Type != DSharpPlus.ChannelType.Voice) throw new Exception("El-canal-de-voz-no-es-válido.");

                var conn = node.GetGuildConnection(channel.Guild);

                if (conn == null) throw new Exception("Conexión-no-realizada.");

                await conn.DisconnectAsync();
                //await ctx.RespondAsync($"Desconectado-de-{channel.Name}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

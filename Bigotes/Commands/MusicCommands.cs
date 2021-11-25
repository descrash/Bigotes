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
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText]string url)
        {
            var lava = ctx.Client.GetLavalink();

            try
            {
                if (lava == null) throw new Exception("Conexión-a-lavalink-no-encontrada.");

                var node = lava.ConnectedNodes.Values.First();

                var connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (connection == null) throw new Exception("Conexión-a-nodos-no-encontrada.");
                
                var loadResult = await node.Rest.GetTracksAsync(url);

                //If something went wrong on Lavalink's end or it just couldn't find anything.                  
                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches) throw new Exception($"Pista-no-encontrada-en-{url}.");

                var track = loadResult.Tracks.First();

                await connection.PlayAsync(track);

                await ctx.RespondAsync($"Reproduciendo-{track.Title}");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("`ERROR` ```Recogiendo-mensaje-de-error:-" + ex.Message + "```");
            }
        }

        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();

            try
            {
                if (lava == null) throw new Exception("Conexión-a-lavalink-no-encontrada.");
                if (!lava.ConnectedNodes.Any()) throw new Exception("Conexión-a-nodos-no-encontrada.");

                var node = lava.ConnectedNodes.Values.First();

                if (channel.Type != DSharpPlus.ChannelType.Voice) throw new Exception("El-canal-de-voz-no-es-válido.");

                await node.ConnectAsync(channel);
                //await ctx.RespondAsync($"Conectado-a-{channel.Name}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Command]
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

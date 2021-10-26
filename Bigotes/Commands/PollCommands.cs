using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class PollCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando para la creación de una encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="titulo">Título de la encuesta</param>
        /// <returns></returns>
        [Command("encuesta")]
        [Description("Comando para realizar una encuesta nueva introduciendo titulo.")]
        public async Task Poll(CommandContext ctx, [Description("Título de la encuesta.")][RemainingText]string titulo)
        {
            #region ATRIBUTOS ENCUESTA
            TimeSpan duration = new TimeSpan();
            DiscordMember author = ctx.Member;
            DiscordChannel embedChannel = ctx.Channel;
            string description = String.Empty;
            string numOptions = "0";
            string urlEncuesta = String.Empty;
            List<DiscordEmoji> emojiOptions = new List<DiscordEmoji>();
            #endregion

            await ctx.Channel.SendMessageAsync("`[PROCESANDO PETICIÓN]` ```Orden-recibida.-Preciso-concretar-duración-en-minutos(m),-horas(h)-o-días(d):```").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            #region Recogida y gestión de la duración de la encuesta
            var durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
            string durationSTR = durationMSG.Result.Content.Trim(), unidadRespuesta;
            int durationINT = 0;

            while (!Regex.IsMatch(durationSTR, @"\d[mhd]"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-00m,-00h-o-00d.```").ConfigureAwait(false);
                durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
                durationSTR = durationMSG.Result.Content.Trim();
            }

            if (durationSTR.Contains("m"))
            {
                durationINT = Int32.Parse(durationSTR.Split('m')[0]);
                duration = new TimeSpan(0, durationINT, 0);
                unidadRespuesta = durationINT == 1 ? "minuto" : "minutos";
            }
            else if (durationSTR.Contains("h"))
            {
                durationINT = Int32.Parse(durationSTR.Split('h')[0]);
                duration = new TimeSpan(durationINT, 0, 0);
                unidadRespuesta = durationINT == 1 ? "hora" : "horas";
            }
            else
            {
                durationINT = Int32.Parse(durationSTR.Split('d')[0]);
                duration = new TimeSpan(durationINT, 0, 0, 0);
                unidadRespuesta = durationINT == 1 ? "día" : "días";
            }
            #endregion

            #region Recogida y gestión del canal en el que realizar la encuesta
            await ctx.Channel.SendMessageAsync("`[PROCESANDO DURACIÓN]` ```Programada-duración-de-" + durationINT + "-" + unidadRespuesta + ".``` ```Detallar-nombre-de-canal-de-encuesta.-Libertad-de-tomar-canal-actual-en-caso-de-no-existir-petición.```").ConfigureAwait(false);

            string channelName = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;

            List<DiscordGuild> guilds = ctx.Client.Guilds.Values.ToList<DiscordGuild>();

            foreach (var guild in guilds)
            {
                foreach (var channel in guild.Channels.Values)
                {
                    String[] tagsID = { "#", "<", ">" };

                    //Comparamos nombre si se ha puesto únicamente el nombre ("general") o el ID en caso de haber puesto el tag entero ("#general")
                    if (channel.Name == channelName.Trim() || channel.Id.ToString() == channelName.Split(tagsID, StringSplitOptions.RemoveEmptyEntries)[0].Trim())
                    {
                        embedChannel = channel;
                    }
                }
            }
            #endregion

            #region Recogida y gestión de la descripción
            await ctx.Channel.SendMessageAsync("`[BUSCANDO CANAL]` ```Canal-de encuesta-seleccionado:-" + embedChannel.Name + ".``` ```Se-precisa-texto-descriptivo:```").ConfigureAwait(false);

            description = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Registro de número de opciones
            await ctx.Channel.SendMessageAsync("`[RECOGIENDO DESCRIPCIÓN]` ```¿Número-de-opciones?```");

            numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            while (!Regex.IsMatch(numOptions, @"\d"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-de-numero-entero-positivo.```").ConfigureAwait(false);
                numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            }
            #endregion

            #region Obtención de emojis para las opciones (reacciones)
            var emojiMSG = await ctx.Channel.SendMessageAsync("`[NÚMERO AJUSTADO]` ```Necesarias-reacciones-a-este-mensaje-con-los-emojis-utilizados-en-cada-opción.```");

            while(emojiOptions.Count < Int32.Parse(numOptions))
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == emojiMSG && x.User == author).ConfigureAwait(false);

                emojiOptions.Add(reactionResult.Result.Emoji);

                await ctx.Channel.SendMessageAsync("```Procesada-opción-" + emojiOptions.Count + ":" + reactionResult.Result.Emoji.Name + "```").ConfigureAwait(false);
            }
            #endregion

            #region Creación de encuesta
            await ctx.Channel.SendMessageAsync("`[OPCIONES RECOGIDAS]` ```Se-han-recogido-las-opciones.-Procesando-encuesta...```");

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = titulo,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = author.DisplayName,
                    IconUrl = author.AvatarUrl
                },
                Description = description,
                Color = DiscordColor.Blue
            };
            
            var pollMessage = await embedChannel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            var msgFinal = "`ENCUESTA " + titulo + " FINALIZADA` ```Resultados:```";

            await pollMessage.RespondAsync(msgFinal + string.Join("\n", results));
            
            //await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
            #endregion
        }
    }
}

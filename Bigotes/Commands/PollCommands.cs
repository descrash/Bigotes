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
        /// Comando para realizar una encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="duration">Duración</param>
        /// <param name="options">Opciones</param>
        /// <returns></returns>
        [Command("poll")]
        [Description("Comando para realizar una encuesta nueva.")]
        public async Task Descartado(CommandContext ctx, [Description("Título de la encuesta.")][RemainingText]string title, [Description("Duración de la encuesta.")]TimeSpan duration, [Description("Opciones (emojis) de la encuesta.")]params DiscordEmoji[] emojiOptions)
        {
            string msg = String.Empty;

            msg = "`[PROCESANDO PETICIÓN]` ```Realizando-embebido-de-prueba-para-encuesta.```";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = string.Join(" ", options)
            };

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }


        [Command("encuesta")]
        [Description("Comando para realizar una encuesta nueva.")]
        public async Task Poll(CommandContext ctx, params DiscordEmoji[] emojiOptions)
        {
            #region ATRIBUTOS ENCUESTA
            TimeSpan duration = new TimeSpan();
            string description = String.Empty;
            DiscordMember author = ctx.Member;
            #endregion

            await ctx.Channel.SendMessageAsync("`[PROCESANDO PETICIÓN]` ```Orden-recibida.-Preciso-concretar-duración-en-minutos(m),-horas(h)-o-días(d):```").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            #region Recogida y gestión de la duración de la encuesta
            var durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);
            string durationSTR = durationMSG.Result.Content.Trim(), unidadRespuesta;
            int durationINT = 0;

            while (!Regex.IsMatch(durationSTR, @"\d[mhd]"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-00m,-00h-o-00d.```").ConfigureAwait(false);
                durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);
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

            await ctx.Channel.SendMessageAsync("`[PROCESANDO DURACIÓN]` ```Programada-duración-de-" + durationINT + "-" + unidadRespuesta + ".``` ```Se-precisa-texto-descriptivo:```").ConfigureAwait(false);

            description = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false)).Result.Content;

            var options = emojiOptions.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Encuesta-de-" + author.DisplayName,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = author.DisplayName,
                    IconUrl = author.AvatarUrl
                },
                Description = string.Join(description, options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
        }
    }
}

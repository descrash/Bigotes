using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class InteractivityCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando para activar la interactividad
        /// de roles (esto es provisional, no funciona bien)
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("roles")]
        public async Task Gestion(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel.Name == "roles").ConfigureAwait(false);

            //await ctx.Channel.SendMessageAsync(message.Result.User.Username + " ha reaccionado con " + message.Result.Emoji);

            switch(message.Result.Emoji.Name)
            {
                case ":knife:":
                    //Cambiar rol a SUSURROS
                    break;

                case ":shield:":
                    //Cambiar rol a VIGILIA
                    break;

                case ":eggplant:":
                    //Cambiar rol a SYLVARI
                    break;
            }
        }


        [Command("respondmessage")]
        public async Task RespondMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("respondreaction")]
        public async Task RespondReaction(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Emoji);
        }
    }
}

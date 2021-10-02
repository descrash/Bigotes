using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
            /*PROBLEMAS*/
            //El tiempo de espera de interactividad tiene su límite, además de que la misma se apaga (deja de escuchar)
            //cuando se ha triggereado la primera vez.
            //Los nombres de los iconos, por culpa del puto ascii, vienen como iconos (:knife: == 🔪), ergo no es posible ponerlo
            //por letras, hay que copiar y pegar como tal los iconos. Supongo que con iconos personalizados ESO NO PASA. Ya lo probaré.
            //No me cambia los roles. Ignoro por qué. El miembro que elige es el mío, el rol bien. Pero no lo hace. Se la suda todo.
            //Tal vez sea cosa de los permisos. Abajo se puede ver que he probado con ID y con nombre de rol.


            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel.Name == "roles").ConfigureAwait(false);

            //await ctx.Channel.SendMessageAsync(message.Result.User.Mention + " ha reaccionado con " + message.Result.Emoji);

            switch(message.Result.Emoji.Name)
            {
                case "🔪":
                    //await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(893873018739228712)).ConfigureAwait(false);
                    //await ctx.Member.GrantRoleAsync(ctx.Guild.Roles.Values.Where(x => x.Name == "Susurros").First()).ConfigureAwait(false);
                    break;

                case ":shield:":
                    await ctx.Member.GrantRoleAsync(ctx.Guild.Roles.Values.Where(x => x.Name == "Vigilia").First()).ConfigureAwait(false);
                    break;

                case ":eggplant:":
                    await ctx.Member.GrantRoleAsync(ctx.Guild.Roles.Values.Where(x => x.Name == "Sylvari").First()).ConfigureAwait(false);
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

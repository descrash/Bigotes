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
        /// de roles. IMPORTANTE QUE TENGA PERMISOS PARA
        /// GESTIONAR ROLES
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("roles")]
        [Description("Comando para activar la escucha de roles.")]
        public async Task GestionRoles(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("`[ROLES PREPARADOS]` ```Roles-preparados. A-continuación, escribir-el-mensaje-sobre-la-misma-gestión-de-roles:```").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel.Name == "⚙-roles").ConfigureAwait(false);

            DiscordRole rol;

            //await ctx.Channel.SendMessageAsync(message.Result.User.Mention + " ha reaccionado con " + message.Result.Emoji);

            #region Lista de iconos y roles
            switch (message.Result.Emoji.Name)
            {
                case ":priorato:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Priorato").First();
                    break;

                case ":susurros:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Susurros").First();
                    break;

                case ":vigilia:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Vigilia").First();
                    break;

                case ":HojaBrillante:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Fuerzas Krytenses").First();
                    break;

                case ":charr:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Altas Legiones").First();
                    break;

                case ":sylvari:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Sylvari").First();
                    break;

                case ":mundo:":
                    rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Narradores").First();
                    break;

                default:
                    rol = null;
                    break;
            }
            #endregion

            if (rol != null)
            {
                await ctx.Member.GrantRoleAsync(rol).ConfigureAwait(false);
            }
        }
    }
}

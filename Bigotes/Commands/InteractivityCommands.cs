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
            #region ATRIBUTOS
            DiscordMember author = ctx.Member;
            DiscordMessage messageWithRoles;

            List<string> idsEmojis = new List<string>();
            List<string> idsRoles = new List<string>();

            //Lista de mensajes guardados para su borrado posterior
            List<DiscordMessage> msgsToDelete = new List<DiscordMessage>();
            #endregion

            msgsToDelete.Add(await ctx.Channel.SendMessageAsync("`[PROCESANDO...]` ```Preparando-roles. Necesario-responder-al-mensaje-sobre-el-que-colocar-los-emoticonos.```").ConfigureAwait(false));

            var interactivity = ctx.Client.GetInteractivity();

            var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
            messageWithRoles = answer.Result.ReferencedMessage;
            msgsToDelete.Add(answer.Result);

            msgsToDelete.Add(await ctx.Channel.SendMessageAsync("`[MENSAJE REGISTRADO]` ```Comenzando-registro-de-opciones.```").ConfigureAwait(false));
            
            do
            {
                msgsToDelete.Add(await ctx.Channel.SendMessageAsync("`[REGISTRO DE OPCIÓN]` ```Se-solicita-mensaje-con-opción-en-formato- 'EMOJI-@ROL'.```").ConfigureAwait(false));
                answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
                msgsToDelete.Add(answer.Result);

                var strArray = answer.Result.Content.Trim().Split('-');
                var idEmoji = strArray[0]; //Pendiente de cambio por atachment
                var idRole = strArray[1];
                idsEmojis.Add(idEmoji);
                idsRoles.Add(idRole);

                msgsToDelete.Add(await ctx.Channel.SendMessageAsync("`[OPCIÓN REGISTRADA]` ```¿Son-necesarias-más-opciones? Si/No```").ConfigureAwait(false));
                answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
                msgsToDelete.Add(answer.Result);

            } while (answer.Result.Content.Trim().ToUpper() != "NO");

            

            //await ctx.Channel.SendMessageAsync("`[ROLES PREPARADOS]` ```Roles-preparados. A-continuación, escribir-el-mensaje-sobre-la-misma-gestión-de-roles:```").ConfigureAwait(false);

            //var message = await interactivity.WaitForReactionAsync(x => x.Channel.Name == "⚙-roles").ConfigureAwait(false);

            //DiscordRole rol;

            //await ctx.Channel.SendMessageAsync(message.Result.User.Mention + " ha reaccionado con " + message.Result.Emoji);

            #region Lista de iconos y roles
            //switch (message.Result.Emoji.Name)
            //{
            //    case ":priorato:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Priorato").First();
            //        break;

            //    case ":susurros:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Susurros").First();
            //        break;

            //    case ":vigilia:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Vigilia").First();
            //        break;

            //    case ":HojaBrillante:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Fuerzas Krytenses").First();
            //        break;

            //    case ":charr:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Altas Legiones").First();
            //        break;

            //    case ":sylvari:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Sylvari").First();
            //        break;

            //    case ":mundo:":
            //        rol = ctx.Guild.Roles.Values.Where(x => x.Name == "Narradores").First();
            //        break;

            //    default:
            //        rol = null;
            //        break;
            //}
            #endregion

            //if (rol != null)
            //{
            //    await ctx.Member.GrantRoleAsync(rol).ConfigureAwait(false);
            //}
        }
    }
}

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class PollCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando para crear un mensaje cuyos iconos puedan
        /// afectar distintos aspectos (roles, votaciones, etc)
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("lista")]
        public async Task Lista(CommandContext ctx, string opcion)
        {
            var interactivity = ctx.Client.GetInteractivity();

            switch (opcion)
            {
                case "de roles":

                    break;
            }

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("`[ERROR]` ```Opción-no-implementada.```").ConfigureAwait(false);
        }

        /// <summary>
        /// Comando para realizar una encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("encuesta")]
        public async Task Poll(CommandContext ctx)
        {
            //var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendMessageAsync("`[ERROR]` ```Opción-no-implementada.```").ConfigureAwait(false);
        }

        /// <summary>
        /// Comando de espera y comprobación
        /// de procedencia de mensaje (PRUEBA)
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        //[Command("responde")]
        //public async Task Responde(CommandContext ctx)
        //{
        //    var interactivity = ctx.Client.GetInteractivity();

        //    //El mensaje es del mismo canal quel cliente
        //    var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);
        //    //interactivity.WaitForReactionAsync <<<=== esperar por emoji/reacción en lugar de mensaje

        //    await ctx.Channel.SendMessageAsync(message.Result.Content);
        //}
    }
}

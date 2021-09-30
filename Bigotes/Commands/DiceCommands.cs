using Bigotes.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class DiceCommands : BaseCommandModule
    {
        /// <summary>
        /// Comando de tiradas de dados
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [Command("roll")]
        public async Task Roll(CommandContext ctx, string input)
        {
            string output;
            Dices dices = new Dices();

            output = dices.Roll(input).ToString();
            
            if (output == "-1")
            {
                ctx.Channel.SendMessageAsync("`[CARGANDO MENSAJE DE ERROR]` ```Error en ejecución de comando.``` `[CARGANDO SUGERENCIA]` ```Por favor, utilice el formato 00d00 (opcional +/- 00).```").ConfigureAwait(false);
            }
            else
            {
                ctx.Channel.SendMessageAsync("`[CARGANDO RESULTADO]` ```Resultado: " + output + ".```").ConfigureAwait(false);
            }
        }
    }
}

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
        [Description("Efectúa una tirada de dados con el formato 00d00 +/- 00.")]
        [RequireRoles(RoleCheckMode.Any, "@everyone")]
        public async Task Roll(CommandContext ctx, [Description("Tirada de dados.")]string tirada)
        {
            string output, msg;
            Dices dices = new Dices();

            output = dices.Roll(tirada).ToString();

            switch(output)
            {
                case "-1":
                    msg = "`[CARGANDO MENSAJE DE ERROR]` ```Error-en-ejecución-de-comando.``` `[CARGANDO SUGERENCIA]` ```Por-favor,-utilice-el-formato-00d00-sumando-de-cero-a-infinito-añadidos).```";
                    break;

                case "1":
                    msg = "`[CARGANDO RESULTADO DESASTROSO]` ```Resultado: " + output + ".``` `[ERROR EN MENSAJE DE ÁNIMO]` ```Aconsejable-realizar-la-creación-de-una-ficha-nueva.```";
                    break;

                default:
                    msg = "`[CARGANDO RESULTADO]` ```Resultado: " + output + ".```";
                    break;
            }

            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
        }
    }
}

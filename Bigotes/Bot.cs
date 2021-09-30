using Bigotes.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bigotes
{
    public class Bot
    {
        #region Propiedades
        /// <summary>
        /// Cliente de Discord
        /// </summary>
        public DiscordClient Client { get; private set; }

        /// <summary>
        /// Extensión de comandos
        /// </summary>
        public CommandsNextExtension Commands { get; private set; }
        #endregion

        #region Métodos
        /// <summary>
        /// Método asíncrono para iniciar el cliente
        /// con la configuración completa.
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            #region Obtención de fichero de configuración (JSON)
            string jsonSTR = string.Empty;
            ConfigJSON ConfigJson;

            using(var fs = File.OpenRead("config.json"))
            {
                using(var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    jsonSTR = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }

            ConfigJson = JsonConvert.DeserializeObject<ConfigJSON>(jsonSTR);
            #endregion

            #region Configuración de bot
            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = ConfigJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            Client = new DiscordClient(config);
            #endregion

            Client.Ready += OnClientReady;

            #region Comandos
            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { ConfigJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,
                IgnoreExtraArguments = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<BasicCommands>();
            Commands.RegisterCommands<DiceCommands>();
            #endregion

            await Client.ConnectAsync();

            //Tiempo de espera extra para dar tiempo a procesamiento de peticiones
            await Task.Delay(-1);
        }

        /// <summary>
        /// Método asíncrono que se inicializa cuando
        /// el cliente está preparado.
        /// </summary>
        /// <param name="s">Este parámetro ha sido añadido en los últimos parches del Nugget</param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnClientReady(DiscordClient s, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}

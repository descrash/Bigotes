using Bigotes.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes.Clases
{
    public class CustomHelpFormatter : DefaultHelpFormatter
    {
        #region Atributos
        protected DiscordEmbedBuilder _embed;
        protected StringBuilder _strBuilder;
        #endregion

        #region Métodos
        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _embed = new DiscordEmbedBuilder();
            _strBuilder = new StringBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _embed.AddField(command.Name, command.Description);
            _strBuilder.AppendLine($"{command.Name} - {command.Description}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            foreach (var cmd in cmds)
            {
                _embed.AddField(cmd.Name, cmd.Description);
                _strBuilder.AppendLine($"**{cmd.Name}** - {cmd.Description}");
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            string titulo = "PANEL DE AYUDA DE BIGOTES";
            string descripcion = "Bienvenidos-al-panel-de-ayuda-de-BIGOTES. A-continuación-se-muestra-la-lista-de-comandos-para-la-correcta-utilización. Para-más-detalle, contactar-con-el-desarrollador-principal.";

            EmbedBuilder.Title = titulo;
            EmbedBuilder.Description = descripcion;
            EmbedBuilder.Color = DiscordColor.Red;
            EmbedBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Patrocinio de Zokab International S.L. (Actualmente en fase de pruebas)"
            };
            EmbedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = Constantes.ICON_BIGOTES
            };
            return base.Build();
        }
        #endregion
    }
}

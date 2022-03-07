using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes.Clases
{
    public class Evento
    {
        #region Propiedades
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public List<string> narradores { get; set; }
        public string fecha { get; set; }
        public string lugar { get; set; }
        public List<string> participantes { get; set; }
        #endregion

        #region MOSTRAR EVENTO
        public StringBuilder Mostrar()
        {
            StringBuilder evento = new StringBuilder();

            #region Formatos
            string pluralN = (narradores == null || narradores.Count == 0) ? "" : "es";
            string narradoresSTR = String.Empty;
            foreach(string _n in narradores)
            {
                narradoresSTR += $" {_n}";
            }

            string pluralP = (participantes == null || participantes.Count == 0) ? "" : "s";
            string participantesSTR = String.Empty;
            foreach (string _p in participantes)
            {
                participantesSTR += $" {_p}";
            }
            #endregion

            try
            {
                evento.Append($"**Nombre del evento**: *{nombre}*\n");
                evento.Append($"**Narrador{pluralN}**:{narradoresSTR}\n");
                evento.Append($"**Fecha y hora**: *{fecha}*\n");
                evento.Append($"**Lugar**: *{lugar}*\n");
                evento.Append($"**Participante{pluralP}**:{participantesSTR}\n\n");
                evento.Append($"**Resumen**: *{descripcion}*");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return evento;
        }
        #endregion
    }
}

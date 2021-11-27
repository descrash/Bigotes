using Bigotes.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes.Clases
{
    public class Ficha
    {
        #region Datos básicos
        public string nombre_completo { get; set; }
        public string apellidos { get; set; }
        public Utiles.Genero genero { get; set; }
        public int bird_year { get; set; }
        public Utiles.Raza raza { get; set; }
        public string ocupacion { get; set; }
        #endregion

        #region Datos físicos
        public string descripcion_fisica { get; set; }
        public double altura { get; set; }
        public double peso { get; set; }
        public string condicion_fisica { get; set; }
        public string color_ojos { get; set; }
        public string color_pelo { get; set; }
        public string rasgos_caracteristicos { get; set; }
        #endregion

        #region Datos psicológicos
        public string descripcion_psicologica { get; set; }
        public string disgustos { get; set; }
        public string habilidades { get; set; }
        public string debilidades { get; set; }
        #endregion

        #region Historia personal
        public string historia_personal { get; set; }
        #endregion

        #region Características
        public int FUERZA { get; set; }
        public int DESTREZA { get; set; }
        public int INTELIGENCIA { get; set; }
        public int CARISMA { get; set; }
        public int PERCEPCION { get; set; }
        public int MAGIA { get; set; }
        public int TOTALES
        {
            get
            {
                return FUERZA + DESTREZA + INTELIGENCIA + CARISMA + PERCEPCION + MAGIA;
            }
        }

        public string descriptorFUERZA { get; set; }
        public string descriptorDESTREZA { get; set; }
        public string descriptorINTELIGENCIA { get; set; }
        public string descriptorCARISMA { get; set; }
        public string descriptorPERCEPCION { get; set; }
        public string descriptorMAGIA { get; set; }
        #endregion

        #region MOSTRAR FICHA
        public StringBuilder Mostrar()
        {
            StringBuilder ficha = new StringBuilder();

            try
            {
                #region DATOS BÁSICOS
                ficha.Append("**DATOS BÁSICOS**\n");
                ficha.Append($"Nombre: *{nombre_completo}*\n");
                ficha.Append($"Apellidos: *{apellidos}*\n");
                ficha.Append($"Género: *{genero.ToString()}*\n");
                ficha.Append($"Año de nacimiento: *{bird_year}*\n");
                ficha.Append($"Raza: *{raza.ToString()}*\n");
                ficha.Append($"Ocupación: *{ocupacion}*\n\n");
                #endregion

                #region DATOS FÍSICOS
                ficha.Append("**DATOS FÍSICOS**\n");
                ficha.Append($"Descripción física: *{descripcion_fisica}*\n");
                ficha.Append($"Altura: *{altura} m*\n");
                ficha.Append($"Peso: *{peso} kg*\n");
                ficha.Append($"Condición física: *{condicion_fisica}*\n");
                ficha.Append($"Color de ojos: *{color_ojos}*\n");
                ficha.Append($"Color del pelo: *{color_pelo}*\n");
                ficha.Append($"Rasgos característicos: *{rasgos_caracteristicos}*\n\n");
                #endregion

                #region DATOS PSICOLÓGICOS
                ficha.Append("**DATOS PSICOLÓGICOS**\n");
                ficha.Append($"Descripción psicológica: *{descripcion_psicologica}*\n");
                ficha.Append($"Disgustos: *{disgustos}*\n");
                ficha.Append($"Habilidades: *{habilidades}*\n");
                ficha.Append($"Debilidades: *{debilidades}*\n\n");
                #endregion

                #region HISTORIA PERSONAL
                ficha.Append("**HISTORIA PERSONAL**\n");
                ficha.Append($"*{historia_personal}*\n\n");
                #endregion

                #region CARACTERÍSTICAS
                ficha.Append("**CARACTERÍSTICAS**\n");
                ficha.Append($"Fuerza: *\"{descriptorFUERZA}\" *{FUERZA}**\n");
                ficha.Append($"Destreza: *\"{descriptorDESTREZA}\" *{DESTREZA}**\n");
                ficha.Append($"Inteligencia: *\"{descriptorINTELIGENCIA}\" *{INTELIGENCIA}**\n");
                ficha.Append($"Carisma: *\"{descriptorCARISMA}\" *{CARISMA}**\n");
                ficha.Append($"Percepción: *\"{descriptorPERCEPCION}\" *{PERCEPCION}**\n");
                ficha.Append($"Magia: *\"{descriptorMAGIA}\" *{MAGIA}**\n");
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ficha;
        }
        #endregion
    }
}

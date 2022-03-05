using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Bigotes.Util
{
    /// <summary>
    /// Clase para el cálculo de las tiradas de dados
    /// </summary>
    public class Dices
    {
        #region Propiedades
        /// <summary>
        /// Función para realizar la cuenta aleatoria
        /// </summary>
        private Random rand { get; set; }

        /// <summary>
        /// Número de dados en cada grupo (2d20 -> diceNumber==2)
        /// </summary>
        private int[] diceNumber { get; set; }

        /// <summary>
        /// Valor del dado a tirar (2d20 -> diceRoll==20)
        /// </summary>
        private int[] diceRoll { get; set; }

        /// <summary>
        /// Valor final de la tirada (2d20 -> diceValue==RESULTADO DE TIRADA)
        /// </summary>
        private int[] diceValue { get; set; }

        /// <summary>
        /// En caso de suma/resta de valores, lista de operadores para saber si suman o resta ('+' '-')
        /// </summary>
        string operadores { get; set; }

        /// <summary>
        /// En caso de suma/resta de valores, lista de operandos de la operación ('1d20', '2', etc)
        /// </summary>
        string[] operandos { get; set; }

        /// <summary>
        /// Valor total de la tirada
        /// </summary>
        private int totalValue { get; set; }
        #endregion

        #region Métodos
        /// <summary>
        /// Método para procesar la cadena de entrada
        /// y realizar la tirada.
        /// DEVUELVE LISTA DE RESULTADOS PARA PROCESAR EN EL MENSAJE
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<string> Roll(string input)
        {
            List<string> RESULTADOS = new List<string>();
            totalValue = 0;
            bool esResta = false, hayPifia = false;
            string[] subStr;
            int numeroDadosTotal = 0;

            try
            {
                rand = new Random();

                #region Comprobación de formato
                if (!Regex.IsMatch(input.Trim(), @"[1-9]\d?d[1-9]\d?(([+-][([1-9]\d?d[1-9]\d?)([1-9]\d?)])*"))
                {
                    throw new Exception("Error en el formato de entrada.");
                }
                #endregion

                #region Obtención de operadores
                foreach (char c in input.Trim())
                {
                    if(c == '+' || c == '-')
                    {
                        operadores += c;
                    }
                }
                operandos = input.Trim().Split('+', '-');
                #endregion

                //Inicialización
                diceNumber = new int[operandos.Length];
                diceRoll = new int[operandos.Length];
                diceValue = new int[operandos.Length];

                #region Obtención de operandos y operación
                for (int i = 0; i < operandos.Length; i++)
                {
                    subStr = operandos[i].Split('d');
                    
                    //Comprobación: en caso de ser dado, tendrá varias partes.
                    //En caso contrario, será un entero sin más
                    if (subStr.Length > 1)
                    {
                        diceNumber[i] = Int32.Parse(subStr[0]);
                        diceRoll[i] = Int32.Parse(subStr[1]);

                        numeroDadosTotal += diceNumber[i];

                        for(int j = 0; j < diceNumber[i]; j++)
                        {
                            int resultado = rand.Next(1, diceRoll[i]);
                            diceValue[i] += resultado;

                            if (diceNumber[i] == 1 && resultado == 1)
                            {
                                //Recogemos la pifia para su comprobación
                                diceValue[i] = resultado;
                                hayPifia = true;
                            }
                            else
                            {
                                hayPifia = false;
                            }
                            RESULTADOS.Add("[" + resultado + "]");
                        }
                    }
                    else
                    {
                        diceValue[i] = Int32.Parse(subStr[0]);
                    }

                    //En caso de que sea un solo dado y saque pifia, se quitan todos los bonificadores,
                    //devolviendo el valor 1.
                    if (i == operandos.Length - 1 && numeroDadosTotal == 1 && hayPifia)
                    {
                        return RESULTADOS;
                    }

                    if (i != 0)
                    {
                        if (operadores[i-1] == '-')
                        {
                            totalValue -= diceValue[i];
                            esResta = true;
                        }
                    }

                    if (!esResta)
                    {
                        totalValue += diceValue[i];
                    }

                    RESULTADOS.Add("Resultado: [" + totalValue + "]");
                    #endregion
                }
            }
            catch(Exception ex)
            {
                Debug.Write("ERROR: " + ex.Message);
                totalValue = -1;
            }

            return RESULTADOS;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SyP_Tarea4_servidor
{
    /// <summary>
    /// Clase donde se almacenan los datos y lógicas de juego cuyo tratamiento recae en la parte del servidor de la aplicacion
    /// </summary>
    public class Mastermind
    {
        #region Campos
        //variables de clase a las que se acceden desde varios metodos de la misma
        static int _correctos = 0;//variable que guarda los aciertos
        static int _descolocados = 0;//variable que guarda los aciertos pero mal posicionados

        static List<char> _colores = new List<char> { 'R', 'A', 'V', 'Y', 'B', 'M' };//ista que guarda los distintos codigos de color que se pasaran al cliente

        //lista dinamica que recoge el numero dee intentos de cada jugador. En caso de aumentar el numero de jugadores permitidos, deberemos aumentar igualmete
        //el numero de lementos de esta Lista, añadiendo un cero a mayores para asegurarnos de que no tenga fallos de acceso a pisicion inexistente
        static List<int> _intentos = new List<int> { 0, 0, 0 ,0 }; 

        static char[] secuencia = new char[4];//array de 4 caracteres d9onde se almacena la secuencia generada por el servidor
        static char[] prediccion = new char[4];//array de 4 caracteres d9onde se almacena la prediccion generada por el jugador
        #endregion

        #region Metodos
        //metodo que genera una secuenca de 4 elementos, eligiendo aleatoriamente indices de la lista de _colores
        internal static void GenerarCodigo()
        {
           
            Random rand = new Random();
            
            for (int i = 0; i < secuencia.Length; i++)//de 1 a 4
            {
                //mete en la posicion i delarray de chars secuencia el valor que se encuentre en el indice de la lista _colores
                //notese que si se amplia la lista de posibles colores, el programa funcionaria igualmente
                secuencia[i] = _colores.ElementAt(rand.Next(_colores.Count));
                
            }

            //Muestra la secuencia generada por pantalla. Nótese que,dado que el juego es en local, cabe la posibilidad de comentar estas
            //lineas de codigo y hacer que el codigo no se muestre por pantalla,con lo que los jugadores puedan tener una experiencia real de juego incluso en 
            //red local. EN este caso se muestra para facilitar la revision y testeo del mismo
            Console.Write("\tLa secuencia generada es: ");
            foreach (char c in secuencia)
            {
                Console.Write(c.ToString() + ", ");
            }
        }

        //Metodo que recoge la prediccion del usuario y su Id, compara la secuencia propuesta con la generada por el servidor
        //y devuelve una cadena de texto con el resultado de la comparacion
        internal static string ComprobarAciertos(string entrada, int jugadorId)
        {
            //para cada comprobacion, reiniciamos contadores de aciertos y descolocados
            _correctos = 0;
            _descolocados = 0;

            if (entrada.Length < 4)//informamos al servidor en caso de que la cadena recibida no contenga 4 caracteres. Aun asi, lo contamos como intento
            {
                ++_intentos[jugadorId];
                Console.WriteLine($"\tNo ha escrito una predicion valida... lleva {_intentos} intentos");
                
                
            }
            else if (entrada.Length >= 4)//si es mayor o igual a 0
            {
                ++_intentos[jugadorId];//añadimos un intento a la posicion de este jugador en la lista de ids

                prediccion = entrada.Substring(0, 4).ToUpper().ToArray();//recogemos y almacenamos en el array de chars los 4 primeros caracteres

                //Si ambos arrays coinciden, guardamos una X en cada posicion del array posicion, que será lo que indique a los metodos de juego del cliente que el jugador 
                //ha adivinado la secuencia
                if (prediccion.SequenceEqual(secuencia))
                {
                    for (int j = 0; j < prediccion.Length; j++)
                    {
                        prediccion[j] = 'x';
                    }
                }
                else //si los arrays no son coincidentes
                {
                    //generamos 2 arrays copias para poder modificarlsa sin alterar las originales y las rellenamos con las mismas
                    char[] copiaPrediccion = new char[4];
                    char[] copiaSecuencia = new char[4];

                    prediccion.CopyTo(copiaPrediccion, 0);
                    secuencia.CopyTo(copiaSecuencia, 0);
                    
                    
                    for (int i = 0; i < 4; i++)
                    {
                        if (prediccion[i] == secuencia[i])//si los valores almacenados en ambos arrays coinciden en la misma posicion
                        {
                            _correctos++;//aumentamos el valor de correctos

                            //substituimos el valor correspondiente en ambas copias generadas por valores "dummy" o estupidos,
                            //para que no vuelvan a aparecer coincidencias en estas posiciones 
                            copiaPrediccion[i] = '^'; 
                            copiaSecuencia[i] = 'º';
                            
                        }
                    }
                    //a continuacion volvemos a recorrer los arrays, pero en este caso los arrays copia con los valores "dummy" integrados,
                    //para qeu los valores coincidentes ya tratados no vuelvan a aparecer. En este caso comparamos mediante dos bucles for anidado
                    //cada valor del array con todos los del otro, exceptuando cuando los indices son el mismo (esto ya no tendria que wser un problema, dada
                    //la inclusion del valor "dummy" anteriormente. Sin embargo, y dado las vultas qeu le tuve qeu dar para que este bucle anidado
                    //funcionase como debiera, prefiero dejarlo como extra precaucion...o añoranza XD)
                    //En este caso, por cada coincidencia, aumentamos en uno el contador de descolocados
                    for (int i = 0; i < 4; i++)
                    {

                        for (int j = 0; j < 4; j++)
                        {
                            if (copiaPrediccion[i] == copiaSecuencia[j] && i!=j)
                            {
                                _descolocados++;
                                copiaSecuencia[j] = '~';
                            }
                        }
                        

                    }
                      
                } 
            }
            //guardamos la prediccion realizada por el jugador en forma de string, guardando asi solo los 4 primeros char que nos envió
            String prediccionColores = new string(prediccion);

            //componemos el string respuesta que enviaremos al jugador, con la informacion y separadores que este necesita
            //para comprobar y formatear la salida de informacion por su consola
            String respuesta = prediccionColores+ "^" + _correctos+ "^" + _descolocados+ "^" + _intentos[jugadorId];
            return respuesta;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SyP_Tarea4_servidor
{

    /// Programa que recrea el clásico juego del MasterMind, donde los jugadores tiene que descifrar un codigo de cuatro elementos, cada uno de ellos puede 
    /// tomar uno de los 6 posibles valores disponibles. El servidor genera esta combinacion de forma aleatoria, recoge las predicciones que realizan los jugadores,
    /// las compara con su combinacion y devuelve al jugador la información de cuantos ha acertado en la posicion correcta, cuantos ha acertado pero no están en la 
    /// posicion correcta, y por ultimo cuantos intentos lleva realizados. Cuando el jugador acierte la combinacion secreta, le informará y éste tendrá que cerrar su aplicacion.
    /// Programa que recrea el clásico juego del MasterMind, donde los jugadores tiene que descifrar un codigo de cuatro elementos, cada uno de ellos puede 
    /// 
    /// 
    ///El servidor, una vez arrancado, realiza todas estas operaciones de forma autónoma, sin 
    ///necesidad de ningun operador que vaya introduciendo parámetros o datos. El solo maneja la comunicacion y recibe datos de clientes, efectua operaciones <summary>
    ///y devuelve una respuesta.
    /// 
    /// El servidor actual soporta hasta 3 jugadores. Este numero es unicamente por razones de facilidad de testeo. Realmente puede admitir muchos mas jugadores,
    /// para modificar este aspecto, solo hay que modificar la variable _intJugadoresMaximos de la clase programa al numero deseado, asi como añadir a la lista
    /// de Integers _intentos de la clase Mastermind el mismo numero de ceros que el de la varible _inJugadoresMaximos mas uno extra. COn esto deberia funcionar
    /// para ese nuevo numoer de jugadores tampoco le pongas 1000, que seguramente la lies, sobretodo teniendo en cuenta que funciona en red local.
    /// 
    /// Cuando se excede el numero de usuarios conectados, el jugador parece que accede, pero al intentar mandar una prediccion, el servidor le informa de la
    /// eventualidad de que está lleno y le indica que reinicie la aplicacion mas tarde para ver si algun jugadro ha salido del servidor


    /// Clase principal de la aplicacion de servidor. En ella se establece el servidor, se manejan las comunicaciones con los jugadores y se llaman a las 
    /// funciones y métodos que desarrolan el juego, llamandolos a su clase.
    class Program
    {
        #region Camposw
        //variables de clase a los que se accede desde varios métodos de la misma
        static int _intNumJugadores = 0;//guarda la cantidad de jugadores conectados al servidor
        static int _intJugadoresMaximos = 3;//variaben la que indicamos elnumero maximo de conexiones que permitimos realizar al servidor

        //listas dinamicas qeu nos sirven para guardar ordenados los nombres y los id de jugador de los usuarios conectados
        static List<Socket> _skJugador = new List<Socket>();
        static List<int> _intJugadorId = new List<int>();
        #endregion

        #region metodos
        /// <summary>
        /// metodo main desde donde arranca el servidor , se comunica con los clientes y se van llamando a los metodos de juego segun vaya siendo necesario
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Acc.PrintCabecera();//imprimimos la cabecera prediseñada desde el metodo Acc

            Task.Run(() => Salir());

            Console.Clear();

            Acc.PrintENcabezado();

            Mastermind.GenerarCodigo();//genera el codigo aleatorio dentro de la clase Mastermind

            Acc.ImprimirLineaColorTexto("\n\t***Pulsa 'S' para apagar el servidor***", ConsoleColor.Red);

            Task.Run(() => Salir());

            try
            {
                //definimos la direccion Ip que tendrá el servidor , abrimos un protocolo Tcp de escucha con la Ip y el puerto indicados y lo arrancamos
                IPAddress ipAd = IPAddress.Parse("127.0.0.1");
                TcpListener myList = new TcpListener(ipAd, 1234);
                myList.Start();

                //informamos por pantalla que el servidor está escuchando
                Console.WriteLine("\n\tEl servidor está funcionando en el puerto 1234...");
                Console.WriteLine("\tLocal EndPoint  :" + myList.LocalEndpoint);
                Console.WriteLine("\tEsperando por jugadores...");
                Task.Run(() => Salir());

                while (true)//cuando un usuario intenta conectarse
                {
                    //mientras no se supere el numero de conexiones permitidas, el servidor abrirá un socket de comunicacion
                    //con ese cliente, le asigna un Id de jugador secuencial, añade el socket y el id a las listas dinamicas
                    //y crea y arranca un hilo que ejecute el metodo de manejo de dicho usuario
                    if (_intNumJugadores < _intJugadoresMaximos)
                    {
                        Socket skJugador = myList.AcceptSocket();
                        int intJugadorId = ++_intNumJugadores;
                        Console.WriteLine("\tConexión aceptada desde " + skJugador.RemoteEndPoint);

                        _skJugador.Add(skJugador);
                        _intJugadorId.Add(intJugadorId);

                        Thread thrJugador = new Thread(() => ManejarCliente(skJugador, intJugadorId));
                        thrJugador.Start();

                        Task.Run(() => Salir());
                    }
                    else
                    {
                        //si un usuario intenta conectarse y el servidor está lleno, cuando éste intente enviar unaprediccion,
                        //el servidor le indicará que no tiene capacidad para mas usuarios y le indica que finalize el program y regrese mas tarde
                        //para ello abre un socket y un flujo asociado al mismo para enviarle dicha informacion, y a continuacion cierra ambos, flujo y socket
                        //y pone el hilo en espera por 5 segundos para aceptar nuevas conexiones para darle tiempo al jugadro a recibir la informacion
                        Console.WriteLine("\tJugadores intentando conectarse. En cola hasta que se desconecte un jugador activo");
                        Socket skJugador = myList.AcceptSocket();
                        NetworkStream stream = new NetworkStream(skJugador);
                        byte[] message = Encoding.ASCII.GetBytes("En cola, servidor lleno, reintentando ocnexion en unos momentos.");
                        stream.Write(message, 0, message.Length);
                        stream.Close();
                        skJugador.Close();
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (Exception ex)//excepcion para el caso de que no se pueda efectuar el incio del servidor
            {
                Console.WriteLine("\tError... " + ex.StackTrace);
            }
            Task.Run(() => Salir());
            Console.ReadKey();
        }

        /// <summary>
        /// metodo que usa el servidor para manejar la conexion con el cliente
        /// </summary>
        /// <param name="skJugador"></param>socket de comunicacion con el jugador para el que se aplica el metodo
        /// <param name="intJugadorId"></param>id asociado al socket, que sirve para diferenciarlos una vezque empiezen a comunicarse con el servidor
        public static void ManejarCliente(Socket skJugador, int intJugadorId)
        {
            try
            {
                byte[] bytes = new byte[1024];//array de bytes donde se almacenarán los datos recibidos
                int bytesRec;//variable donde se almacenan el numero de bytes recibidos

                while ((bytesRec = skJugador.Receive(bytes)) > 0)//mientras el socket no este vacio
                {
                    //se recoge la informacion del flujo, se parsea a string y se informa de la operacion por consola
                    Console.WriteLine($"\tRecibido del jugador {intJugadorId}");
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine($"\tDatos recibidos del jugador {intJugadorId} "+data);

                    //se comprueba la informacion recibida por medio del metodo del juego, pasando la informacion y el id del jugador como parametros
                    //y se guarda el resultado en un string
                    string comprobacion = Mastermind.ComprobarAciertos(data, intJugadorId);

                    //se genera un flujo de bytes para responder al cliente la informacion guardada en el string generado
                    ASCIIEncoding asen = new ASCIIEncoding();
                    skJugador.Send(asen.GetBytes(comprobacion));
                    Console.WriteLine($"\tServidor acaba de enviar respuesta al jugador{intJugadorId} ");
                    
                    Task.Run(() => Salir());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\tNO se pudo manejar al cliente");
            }
            //cuando el flujo esté vacio, se ciuerra el socket, se eliminan de las listas de sockets y ID de jugadores los datos
            //asociados a esta conexion y se reduce el numero de jugadores conectados para permitir entrada a otros
            finally
            {
                skJugador.Close();
                _skJugador.Remove(skJugador);
                _intJugadorId.Remove(intJugadorId);
                _intNumJugadores--;
            }
        }

        public static void Salir() 
        {
            char respuesta = (char)Console.ReadKey().KeyChar;
            if(respuesta == 's') 
            {
                Environment.Exit(0);
            }
        }
        #endregion

        #region Propiedades
        //propiedades de las variables de clase para utilizarlas en metodos de otras clases
        public static List<Socket> SkJugador { get => _skJugador; set => _skJugador = value; }
        public static List<int> IntJugadorId { get => _intJugadorId; set => _intJugadorId = value; }
        #endregion
    }
}


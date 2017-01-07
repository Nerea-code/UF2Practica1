using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;

namespace UF2Practica1 //Alumna: Nerea Tomás
{
	class MainClass
    {
        #region Valors constants
        const string fitxer = "CuaClients.csv"; //arxiu que conté informació clients
        public static int totalCaixeres = 0; //total Caixeres que disposa el programa
        public static ConcurrentQueue<Client> cua = new ConcurrentQueue<Client>();
        /*cua concurrent per poder gestionar els diversos clients de l'arxiu
          pública i estàtica (xq és una única cua comuna per a tot el programa)
          Tres mètodes bàsics: 
            - Enqueue method: per afegir item al final de la cua
            - TryPeek method: intenta obtenir un item de la cua sense eliminar-lo de la llista
            - TryDequeue method: intenta obtenir un item de la cua i l'elimina de la llista
                ambdós retornen True/False alhora que l'item en qüestió a través del paràmetre out
                bool isSuccessful = coll.TryDequeue(out item);
        */
        #endregion

        public static void Main(string[] args)
		{
            int resposta;
            bool respostaOk = false;
            Console.WriteLine("EXERCICI SUPERMERCAT");
            Console.WriteLine("En el supòsit de que un supermercat té un total de fins a 8 caixes registradores...");
            do
            {
                //preguntem de quantes caixeres disposem.
                Console.WriteLine("De quantes caixeres disposem avui?");
                try
                {
                    //controlem que el valor entrat per teclat sigui un numèric.
                    resposta = int.Parse(Console.ReadLine());
                    //i que sigui entre 1 i 8.
                    if (resposta >= 1 && resposta <= 8)
                    {
                        totalCaixeres = resposta;
                        respostaOk = true;
                    }
                    else
                    {
                        Console.WriteLine("Opció no vàlida.");
                        Console.WriteLine("El valor expressat ha de ser entre 1 i 8.");
                    }
                
                }
                catch (Exception)
                {
                    Console.WriteLine("El valor introduït no és numèric.");
                }
            } while (respostaOk == false);


            var clock = new Stopwatch(); //rellotge x controlar el temps que tarda en gestionar-se tota la cua
            var threads = new List<Thread>();  //llista de threads per poder controlar els diversos fils

            //Codi per poder tenir l'arxiu CSV al directori del projecte
            String currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(currentDirectory);
            //Tirem dos directoris enrere per sortir de bin/debug fins el directori del projecte
            String ruta = currentDirectoryInfo.Parent.Parent.FullName;
            ruta = Path.Combine(ruta, fitxer);

			try
			{
				//obrim l'arxiu per llegir-lo
                var reader = new StreamReader(File.OpenRead(@ruta)); 

				//Carreguem el llistat de clients a la cua concurrent
                //que permet que els diferents threads accedeixin a la cua sense problemes de concurrència. 
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line.Split(';');
                    //creem un nou client passant-li els seus paràmetres
					var client = new Client() {nom = values[0], nProductes = Int32.Parse(values[1])};
					//afegim el client a la cua concurrent
                    cua.Enqueue(client);
				}

			}
			catch (Exception)
			{
				Console.WriteLine("Error accedint a l'arxiu");
				Console.ReadKey();
				Environment.Exit(0);
			}

			clock.Start();


			//Instanciar les caixeres i afegir el thread creat a la llista de threads
            for (int i = 1; i <= totalCaixeres; i++)
            {
                var caixera = new Caixera() { idCaixera = i };
                var fil = new Thread(()=>caixera.gestionarCua()); //operadors lambda
                fil.Start();
                threads.Add(fil);
            }


            //Procediment per esperar que acabin tots els threads abans de donar per finalitzat el procés
            //El mètode Join s'usa per assegurar que un thread ha acabat. 
            //Per tant, per cada thread de la llista de Threads, esperarem a que acabi.
			foreach (Thread thread in threads)
				thread.Join();

			//Parem el rellotge i mostrem el temps que s'ha tardat
			clock.Stop();
			double temps = clock.ElapsedMilliseconds / 1000;
			Console.WriteLine("Temps total Task: " + temps + " segons");
			Console.ReadKey();
            //Console.Clear();
		}
	}

	#region ClassCaixera
	public class Caixera
	{
		public int idCaixera
		{
			get;
			set;
		}

		public void gestionarCua()
		{
			//Cada caixera extreu un nou client de la cua per a tractar-lo
            //mentre hi hagi clients a la cua (no estigui buida), agafem un nou client i gestionem el carro 

            while (!MainClass.cua.IsEmpty)
            {
                Client client = new Client();
                MainClass.cua.TryDequeue(out client);
                //no pregunto pel booleà xq si hem entrat al bucle es xq encara hi ha clients a la cua, no?
                gestionarCarro(client);
            }
		}

		private void gestionarCarro(Client client)
		{
            
			Console.WriteLine("La caixera " + this.idCaixera + " comença amb el client " + client.nom + " que té " + client.nProductes + " productes");

            //hi ha un bucle per simular el pas dels diferents articles per l'escàner.
			for (int i = 0; i < client.nProductes; i++)
			{
				this.gestionarProducte();
			}

			Console.WriteLine(">>>>>> La caixera " + this.idCaixera + " ha acabat amb el client " + client.nom);
		}

		private void gestionarProducte()
		{
            //simula el procés de l'escàner i introdueix una espera de 1 s.
            Thread.Sleep(TimeSpan.FromSeconds(1));
		}
    }
	#endregion

	#region ClassClient
	public class Client
	{
		public string nom
		{
			get;
			set;
		}

		public int nProductes
		{
			get;
			set;
		}
	}
	#endregion

}

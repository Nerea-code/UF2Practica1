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
        const int totalCaixeres = 3; //total Caixeres que disposa el programa
        const string fitxer = "CuaClients.csv"; //arxiu que conté informació clients
        
        public static ConcurrentQueue<Client> cua = new ConcurrentQueue<Client>();
        /*cua concurrent per poder gestionar els diversos clients de l'arxiu
          pública i estàtica (xq és una única cua comuna per a tot el programa)
          Tres mètodes bàsics: 
            - Enqueue method: per afegir item al final de la cua
            - TryPeek method: intenta obtenir un item de la cua sense eliminar-lo de la llista
            - TryDequeue method: intenta obtenir un item de la cua i l'elimina de la llista
                bool isSuccessful = coll.TryDequeue(out item);
                ambdós retornen True/False alhora que l'item en qüestió a través del paràmetre out
        */
        #endregion

        public static void Main(string[] args)
		{

            var clock = new Stopwatch(); //rellotge x controlar el temps que tarda en gestionar-se tota la cua
            var threads = new List<Thread>();  //llista de threads per poder controlar els diversos fils

			//Recordeu-vos que el fitxer CSV ha d'estar a la carpeta bin/debug de la solució
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
                //que permet que els diferents Threads accedeixin a la cua sense problemes de concurrència. 
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
            for (int i = 0; i < totalCaixeres; i++)
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
			Console.Clear();
			Console.WriteLine("Temps total Task: " + temps + " segons");
			Console.ReadKey();
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
            Client client;
            bool isSuccessful = MainClass.cua.TryDequeue(out client);
            if (isSuccessful == true){
                gestionarCarro(client);
            }
            
		
            //mentre hi hagi clients a la cua (no estigui buida)
            //agafem un nou client i gestionem el carro d'aquest en qüestió
           
            /* Client client = null;
            while(MainClass.cua.TryDequeue(out client))
            {
               ProcesarCompra(client);
          }*/

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

        public Client client { get; set; }
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

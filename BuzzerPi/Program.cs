using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;

namespace BuzzerPi
{
	class MainClass
	{
		static JsonSerializer serializer = new JsonSerializer ();

		public static void Main (string [] args)
		{
			var deviceClient = DeviceClient.CreateFromConnectionString (Constants.ConnectionString,
			                                                            Constants.DeviceId,
			                                                            TransportType.Http1);
			ProcessMessages (deviceClient);

			Console.ReadLine ();
		}

		async static void ProcessMessages (DeviceClient client)
		{
			await client.OpenAsync ();

			while (true) {
				Message message = null;
				try {
					message = await client.ReceiveAsync ();
				} catch (Exception e) {
					Console.WriteLine ("+++ Exception while receiving");
					Console.WriteLine (e);
					System.Threading.Thread.Sleep (1000);
				}

				if (message == null)
					continue;

				var bytes = message.GetBytes ();
				await Buzzer (client, BitConverter.ToBoolean (bytes, 0));
				await client.CompleteAsync (message);
			}
		}

		async static Task Buzzer (DeviceClient client, bool openDoor)
		{
			Console.WriteLine ("== Buzzing door to state: {0}", openDoor);

			// Send an event back that the door was opened
			var stream = new MemoryStream (10);
			var writer = new BsonWriter (stream);
			serializer.Serialize (writer, new BuzzerState {
				BuzzerId = "home",
				NewState = openDoor,
				OldState = !openDoor
			});

			var message = new Message (stream.ToArray ());
			await client.SendEventAsync (message);
		}
	}
}

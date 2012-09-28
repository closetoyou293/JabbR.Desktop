using System;
using System.Collections.Generic;
using Eto;
using System.Collections.ObjectModel;


namespace JabbR.Eto.Model
{
	public enum BadgeDisplayMode
	{
		None,
		All,
		Highlighted
	}

	public class Configuration : IXmlReadable
	{
		List<Server> servers = new List<Server>();
		
		public IEnumerable<Server> Servers { get { return servers; } }

		public BadgeDisplayMode BadgeDisplay { get; set; }
		
		public event EventHandler<ServerEventArgs> ServerAdded;

		protected virtual void OnServerAdded (ServerEventArgs e)
		{
			if (ServerAdded != null)
				ServerAdded (this, e);
		}
		
		public event EventHandler<ServerEventArgs> ServerRemoved;

		protected virtual void OnServerRemoved (ServerEventArgs e)
		{
			if (ServerRemoved != null)
				ServerRemoved (this, e);
		}
		
		
		public Configuration ()
		{
		}

		class DisconnectHelper
		{
			public int DisconnectCount { get; set; }

			public Action Finished { get; set; }

			public void HookServer (Server server)
			{
				DisconnectCount++;
				if (Finished != null)
					server.Disconnected += Disconnected;
			}

			public void FinishDisconnect ()
			{
				if (DisconnectCount == 0 && Finished != null)
					Finished ();
			}

			public void Disconnected (object sender, EventArgs e)
			{
				var server = sender as Server;
				server.Disconnected -= Disconnected;
				DisconnectCount--;
				if (DisconnectCount == 0)
					Finished ();
			}
		}
		
		public void DisconnectAll (Action finished = null)
		{
			var helper = new DisconnectHelper { Finished = finished };
			foreach (var server in Servers) {
				if (server.IsConnected)
				{
					helper.HookServer (server);
					server.Disconnect ();
				}
			}
			helper.FinishDisconnect ();
		}

		public void RemoveServer (Server server)
		{
			if (server.IsConnected)
				server.Disconnect ();
			servers.Remove (server);
			OnServerRemoved (new ServerEventArgs(server));
		}
		
		public void AddServer (Server server)
		{
			servers.Add (server);
			OnServerAdded (new ServerEventArgs(server));
		}
		
		#region IXmlReadable implementation
		
		public void ReadXml (System.Xml.XmlElement element)
		{
			element.ReadChildListXml(servers, Server.CreateFromXml, "server", "servers");
			this.BadgeDisplay = element.GetEnumAttribute<BadgeDisplayMode> ("badgeDisplay") ?? BadgeDisplayMode.Highlighted;
		}

		public void WriteXml (System.Xml.XmlElement element)
		{
			element.WriteChildListXml(servers, "server", "servers");
			if (this.BadgeDisplay != BadgeDisplayMode.Highlighted)
				element.SetAttribute ("badgeDisplay", this.BadgeDisplay);
		}
		
		#endregion
	}
}


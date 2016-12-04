using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace LoveHue
{
    public class Connection
    {
        private readonly List<Section> _sectionList = new List<Section>();
        public Connection(string ip, string name, string userid)
        {
            this.Ip = ip;
            this.Name = name;
            this.UserId = userid;
        }

        public string Ip { get; private set; }
        public string Name { get; private set; }
        public string UserId { get; private set; }

        public void AddSection(Section section)
        {
            _sectionList.Add(section);
        }

        public List<Section> GetSectionList()
        {
            return _sectionList;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Section
    {
        private List<Lamp> _lampList;
        public Section(string sectionName, List<Lamp> lampList, Color color)
        {
            this.Name = sectionName;
            this._lampList = lampList;
            this.Color = color;
        }
        public string Name { get; private set; }
        public Color Color { get; private set; }

        public void SetLampList(List<Lamp> lampList)
        {
            _lampList = lampList;
        }

        public List<Lamp> GetLampList()
        {
            return _lampList;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Lamp
    {
        public Lamp(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
        public int Id { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return this.Id + " - " +  Name;
        }
    }

    public sealed class Data
    {
        public ObservableCollection<Connection> Connections { get; }

        public void AddConnection(string ip, string name, string userid)
        {
            Connections.Add(new Connection(ip,name,userid));
        }

        public Connection GetConnectionByIp(string ip)
        {
            var matches = Connections.Where(x => x.Ip == ip);
            if (matches.Count() == 1) return matches.First();
            return null;
        }
    }
}


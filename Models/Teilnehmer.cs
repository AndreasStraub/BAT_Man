using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Test.Models
{
    public class Teilnehmer
    {
        public int TeilnehmerID { get; set; }
        public string Kurs { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string RehaNummer { get; set; }
        public string EMail { get; set; }

        public bool MussPasswortAendern { get; set; }
    }
}

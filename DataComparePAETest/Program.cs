using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using Microsoft.XmlDiffPatch;

namespace DataComparePAETest
{
    public class Program
    {
        //Get XML From dataBase 1
        public static string GetNewXml(int id)
        {


            string RetrivedXML = null;

            try
            {
                //Instance for the database
                DatabaseProviderFactory factory = new DatabaseProviderFactory();
                Microsoft.Practices.EnterpriseLibrary.Data.Database db1 = factory.Create("DB1");

                DbCommand dbCommand = db1.GetSqlStringCommand("SELECT xmldata FROM data WHERE id='" + id + "'");
                object getNewXml = db1.ExecuteScalar(dbCommand);

                if (getNewXml != null)
                    RetrivedXML = (string)getNewXml;
                else
                    throw new Exception(String.Format("Could not find the historyId.", id));

                return RetrivedXML;
            }
            catch (Exception)
            {
                throw;
            }

        }


        //Get XML From Database2
        public static string GetOldXml(int id)
        {
            string RetrivedXML2 = null;

            try
            {
                //Instance for the database
                DatabaseProviderFactory factory2 = new DatabaseProviderFactory();
                Microsoft.Practices.EnterpriseLibrary.Data.Database db2 = factory2.Create("DB2");

                DbCommand dbCommand2 = db2.GetSqlStringCommand("SELECT xmldata FROM data WHERE id='" + id + "'");
                object getOldXml = db2.ExecuteScalar(dbCommand2);

                if (getOldXml != null)
                    RetrivedXML2 = (string)getOldXml;
                else
                    throw new Exception(String.Format("Could not find the historyId.", id));

                return RetrivedXML2;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //Function to Compare Two XML's
        private static void CompareXml(XmlNode oldXml, XmlNode newXml, List<string> errors)
        {

            string errorMessage = null;

            if (oldXml.Name != newXml.Name)
            {
                errorMessage = $"Mismatched node name: {GetXPath(oldXml)} vs. {GetXPath(newXml)}";
            }
            else if ((oldXml.HasChildNodes && !newXml.HasChildNodes) || (!oldXml.HasChildNodes && newXml.HasChildNodes))
            {
                errorMessage = $"Mismatched node structure: {GetXPath(oldXml)} vs. {GetXPath(newXml)}";
            }
            else if (oldXml.HasChildNodes && newXml.HasChildNodes)
            {
                IEnumerator<XmlNode> oldIterator = oldXml.ChildNodes.OfType<XmlNode>().GetEnumerator();
                IEnumerator<XmlNode> newIterator = newXml.ChildNodes.OfType<XmlNode>().GetEnumerator();

                while (oldIterator.MoveNext() && newIterator.MoveNext())
                {
                    CompareXml(oldIterator.Current, newIterator.Current, errors);

                }


            }
            else if (oldXml.InnerText != newXml.InnerText)
            {
                errorMessage = $"Mismatched node values: {GetXPath(oldXml)} Vs {GetXPath(newXml)}";

            }

            if (errorMessage != null)
            {
                errors.Add(errorMessage);
            }


        }

        //Get the Path of the XML Node
        private static string GetXPath(XmlNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }
            else if (node.NodeType == XmlNodeType.Text)
            {
                return GetXPath(node.ParentNode);
            }
            else if (node.ParentNode != null && node.ParentNode != node.OwnerDocument)
            {
                return $"{GetXPath(node.ParentNode)}/{node.Name}";
            }
            else
            {
                return $"/{node.Name}";
            }
        }
        //Main Function
        static void Main(string[] args)
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();


            XmlDocument oldXml = new XmlDocument();
            XmlDocument newXml = new XmlDocument();
            List<String> errors = new List<string>();
            int[] value = new int[250];
            value[0] = 460081;
            value[1] = 460082;          

            //To Print Each Corresponding ID's
            foreach (int ids in value)
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(@"C: \xmllog.txt", true);
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------");
                file.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
                errors.Clear();
                int idss;
                idss = Convert.ToInt32(ids);
                Console.WriteLine(idss);
                file.WriteLine(idss);
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------");
                file.WriteLine("----------------------------------------------------------------------------------------------------------------------------");


                //Load XML's
                if (idss != 0)
                {

                    oldXml.LoadXml(GetOldXml(idss));
                    newXml.LoadXml(GetNewXml(idss));
                    CompareXml(oldXml, newXml, errors);

                }

                else
                {
                    Console.WriteLine(" End of List");
                    file.WriteLine(" End of List");
                    break;


                }


                //**************Use This If getting the XML from local system instead of DB*******

                //oldXml.Load(@"C:\Users\telango\Desktop\XMLData19.xml");
                //newXml.Load(@"C:\Users\telango\Desktop\XMLData20.xml");
                //CompareXml(oldXml, newXml, errors);






                file.Close();
                //Print Differences/Result
                if (errors.Count == 0)
                {
                    System.IO.StreamWriter files = new System.IO.StreamWriter(@"C:\Users\Documents\Visual Studio 2015\Projects\Xml compare Log\xmllog.txt", true);
                    Console.WriteLine("Complete Match");
                    files.WriteLine("Complete Match");
                    files.Close();

                }
                foreach (var item in errors)
                {


                    System.IO.StreamWriter files = new System.IO.StreamWriter(@"C:\Users\Documents\Visual Studio 2015\Projects\Xml compare Log\xmllog.txt", true);
                    Console.WriteLine(item);
                    files.WriteLine(item);
                    Console.WriteLine("                                                   ");
                    files.WriteLine("                                                   ");
                    files.Close();

                }

            }
            // The Time Elapsed for Comparing XML's
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);
            Console.ReadKey();
        }

    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Data;
using OxyPlot;
using TFG.src.exceptions;
using Microsoft.Win32;
using TFG.src.ViewModels;

namespace TFG.src.classes
{
	public static class XMLLoader
	{
		//private static XMLLoader myXMLLoader;

		//public static XMLLoader getXMLLoader()
		//{
		//	if (myXMLLoader == null)
		//	{
		//		myXMLLoader = new XMLLoader();
		//	}

		//	return myXMLLoader;
		//}


		/// <summary>
		/// Method that validates an XML with a given XSD
		/// </summary>
		/// <param name="pathToXml"></param>
		/// <param name="pathToXsd"></param>
		/// <returns>A boolean indicating if the given XML complies with the XSD</returns>
		public static bool Validate(string pathToXml, string pathToXsd)
		{

			XmlSchemaSet schemas = new XmlSchemaSet();
			schemas.Add("", XmlReader.Create(new StreamReader(pathToXsd)));

			XDocument xml = XDocument.Load(pathToXml);

			Console.WriteLine("Validating xml");
			bool errors = false;
			xml.Validate(schemas, (o, e) =>
			{
				Console.WriteLine("{0}", e.Message);
				errors = true;
			});
			Console.WriteLine("doc1 {0}", errors ? "did not validate" : "validated");
			return !errors;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pathToXml"></param>
		/// <returns></returns>
		public static LinkedList<AbstractDataVisualizerViewModel> LoadXMLData(string pathToXml)
		{
			LinkedList<AbstractDataVisualizerViewModel> viewModels = new LinkedList<AbstractDataVisualizerViewModel>();

			if (Validate(pathToXml, "schemas/ObservationModelData.xsd"))
			{
				
				XElement xml = XElement.Load(pathToXml);
				//IEnumerable<XElement> valores =
				//	from ins in xml.Descendants("propiedad")
				//	where (string)ins.Attribute("name") == "prop1"
				//	select ins;

				//foreach (XElement instante in valores)
				//{
				//	Console.WriteLine(instante.Parent.Parent.Attribute("numInstante"));
				//	Console.WriteLine("Propiedad: " + instante.Attribute("name") + ", Valor: " + instante.Attribute("value"));
				//}

				//IEnumerable<XElement> observaciones =
				//	from obs in xml.Descendants("observation")
				//	select obs;

				//foreach (XElement obs in observaciones)
				//{
				//	Console.WriteLine(obs.Attribute("name"));
				//	Console.WriteLine("Propiedades de la observacion:");
				//	Console.WriteLine("================================");
				//	IEnumerable<XElement> propiedades =
				//		from prop in obs.Descendants()
				//		select prop;

				//	foreach (XElement prop in propiedades)
				//	{
				//		Console.WriteLine(prop.Attribute("name"));
				//	}
				//	//Console.WriteLine();
				//}

				//Obtenemos los datos de las propiedades y creamos los graficos

				IEnumerable<XElement> propiedadesContinuas = getContinousData(xml);
				process(propiedadesContinuas, true, viewModels);

				IEnumerable<XElement> propiedadesDiscretas = getDiscreteData(xml);
				process(propiedadesDiscretas, false, viewModels);

			}
			else
			{
				throw new FileFormatException();
			}
			return viewModels;
		}


		private static void process(IEnumerable<XElement> propiedades, bool EsContinuo, LinkedList<AbstractDataVisualizerViewModel> viewModels)
		{

			foreach (XElement prop in propiedades)
			{
				IEnumerable<XElement> data =
					from da in prop.Descendants("instant")
					select da;

				ICollection<DataPoint> pointCollection = new LinkedList<DataPoint>();
				Dictionary<string, double> labels = new Dictionary<string, double>(data.Count());

				foreach (XElement instant in data)
				{
					double x = Double.Parse(instant.Attribute("ins").Value);
					double y = 0;
					if (!EsContinuo)
					{
						
						bool success = labels.TryGetValue(instant.Attribute("value").Value, out y);
						if(!success) {
							y = labels.Count;
							labels.Add(instant.Attribute("value").Value, labels.Count);
						}
					}
					else
					{
						y = Double.Parse(instant.Attribute("value").Value);
					}

					pointCollection.Add(new DataPoint(x, y));
				}
				List<DataPoint> points = new List<DataPoint>(pointCollection);
				AbstractDataVisualizerViewModel avm;
				if (EsContinuo)
				{
					avm = new ContinousDataVisualizerViewModel(points);
				}
				else
				{
					avm = new DiscreteDataVisualizerViewModel(points, labels.Keys.ToList());
				}
				viewModels.AddLast(avm);
			}
			
		}

		private static IEnumerable<XElement> getContinousData(XElement xml)
		{
			return 	from prop in xml.Descendants("property")
					where (string)prop.Attribute("type") == "continous"
					orderby (string)prop.Attribute("ins") ascending
					select prop;
		}

		private static IEnumerable<XElement> getDiscreteData(XElement xml)
		{
			return from prop in xml.Descendants("property")
				   where (string)prop.Attribute("type") == "discrete"
				   orderby (string)prop.Attribute("ins") ascending
				   select prop;
		}

		public static void loadXMLModel(string pathToXml)
		{

		}

		public static string openFile()
		{
			string filename = null;
			// Configure open file dialog box
			OpenFileDialog dlg = new OpenFileDialog();
			//dlg.FileName = "Document"; // Default file name
			dlg.Multiselect = false;
			dlg.DefaultExt = ".xml"; // Default file extension
			// Filter files by extension
			dlg.Filter = "XML Files|*.xml";

			// Show open file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process open file dialog box results 
			if (result == true)
			{
				// Open document 
				filename = dlg.FileName;
			}
			else
			{
				throw new FileNotSelectedException();
			}
			return filename;
		}
	}
}	

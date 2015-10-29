﻿//
//  Tortuga - Life Cycle Analysis for McNeel Rhino Grassopper 3D (R)
//  Copyright (C) 2015  Maximilian Thumfart
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml;

namespace Tortuga.Types
{



    public class LifecycleStage
    {
        [DataMember]
        public string Name;

        [DataMember]
        public int Column;

        public override string ToString()
        {
            return this.Name;
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;

            var cnt = new Dictionary<string, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s.ToString()))
                {
                    cnt[s.ToString()]++;
                }
                else
                {
                    cnt.Add(s.ToString(), 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s.ToString()))
                {
                    cnt[s.ToString()]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

    }


    public class Result
    {
        public UnitDouble<LCA.CO2e> GlobalWarmingPotential;
        public UnitDouble<LCA.kgCFC11> DepletionOfOzoneLayer;
        public UnitDouble<LCA.kgSO2> Acidification;
        public UnitDouble<LCA.kgPhostphate> Eutrophication;
        public UnitDouble<LCA.kgNOx> FormationTroposphericOzone;
        public UnitDouble<LCA.MJ> DepletionOfNonrenewbles;

        public override string ToString()
        {
            return string.Format("Tortuga Result ({0} kgCO2e)", this.GlobalWarmingPotential.Value);
        }
    }

    public class Material
    {
        [DataMember]
        public string Name;

        [DataMember]
        public Guid GUID;

        [DataMember]
        public UnitDouble<LCA.CO2e> GlobalWarmingPotential;

        [DataMember]
        public UnitDouble<LCA.kgCFC11> DepletionOfOzoneLayer;

        [DataMember]
        public UnitDouble<LCA.kgSO2> Acidification;

        [DataMember]
        public UnitDouble<LCA.kgPhostphate> Eutrophication;

        [DataMember]
        public UnitDouble<LCA.kgNOx> FormationTroposphericOzone;

        [DataMember]
        public UnitDouble<LCA.MJ> DepletionOfNonrenewbles;

        [DataMember]
        public List<LifecycleStage> Stages;

        public Material() 
        {

        }

        public ListViewItem Draw()
        {
            ListViewItem layerItem = new ListViewItem();
            layerItem.Margin = new Thickness(1);
           // if (Layer.contextMenu == null) Layer.BuildContextMenu();
            layerItem.Background = Brushes.White;




            layerItem.Tag = this;

            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 20
            };


            layerItem.BorderBrush = Brushes.White;
            layerItem.BorderThickness = new Thickness(1);


            TextBlock title = new TextBlock()
            {
                Text = String.Format("{0} ({1} kgCO2e/m3)",new string[]{this.Name, this.GlobalWarmingPotential.Value.ToString()}),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(5)
            };


            if (this.GlobalWarmingPotential.Value == 0)
            {
                Image img = new Image();
                img.Source = Utilities.GetSourceFromBitmap(Properties.Resources.error);
                img.Height = 16; img.Width = 16;
                panel.Children.Add(img);
            }

            panel.Children.Add(title);




            panel.Margin = new Thickness(2);

            layerItem.Content = panel;

            return layerItem;

        }

        public void CopyFrom(Material source)
        {
            this.Acidification = source.Acidification;
            this.GlobalWarmingPotential = source.GlobalWarmingPotential;
            this.Eutrophication = source.Eutrophication;

            this.FormationTroposphericOzone = source.FormationTroposphericOzone;
            this.DepletionOfNonrenewbles = source.DepletionOfNonrenewbles;
            this.DepletionOfOzoneLayer = source.DepletionOfOzoneLayer;

            this.Stages = source.Stages;
        }

        public static SortedDictionary<string, Material> LoadedMaterials = new SortedDictionary<string, Material>();

        private static Tortuga.Types.Material GetMat(string matname, string id, List<Tortuga.Types.LifecycleStage> Stages)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            string data = client.DownloadString("http://www.oekobaudat.de/OEKOBAU.DAT/resource/processes/" + id + "?format=xml");

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);

            XmlNamespaceManager manager = new XmlNamespaceManager(xml.NameTable);
            manager.AddNamespace("p", "http://lca.jrc.it/ILCD/Process");
            manager.AddNamespace("epd", "http://www.iai.kit.edu/EPD/2013");
            manager.AddNamespace("common", "http://lca.jrc.it/ILCD/Common");
            manager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            Tortuga.Types.Material mat = new Tortuga.Types.Material() { Name =matname , GUID = new Guid(id), Stages = Stages };
            mat.GlobalWarmingPotential = new UnitDouble<LCA.CO2e>(0);
            mat.Acidification = new UnitDouble<LCA.kgSO2>(0);
            mat.DepletionOfNonrenewbles = new UnitDouble<LCA.MJ>(0);
            mat.DepletionOfOzoneLayer = new UnitDouble<LCA.kgCFC11>(0);
            mat.Eutrophication = new UnitDouble<LCA.kgPhostphate>(0);
            mat.FormationTroposphericOzone = new UnitDouble<LCA.kgNOx>(0);


            XmlNodeList xnList = xml.SelectNodes("//p:LCIAResults", manager);
            foreach (XmlNode child in xnList[0].ChildNodes)
            {
                

                if (child.ChildNodes[0].ChildNodes.Count == 1)
                {
                    Dictionary<string, double> values = new Dictionary<string, double>();

                    string Name = child.ChildNodes[0].ChildNodes[0].InnerText;


                    foreach (XmlNode inner in child.ChildNodes)
                    {

                        if (inner.Name == "common:other")
                        {
                            foreach (XmlNode vals in inner.ChildNodes)
                            {
                                if (vals.Name == "epd:amount")
                                {
                                    double tmp = 0;
                                    if (!double.TryParse(vals.InnerText, out tmp)) { tmp = 0; }

                                    if (!values.ContainsKey(vals.Attributes[0].Value))
                                        values.Add(vals.Attributes[0].Value, tmp);
                                    else
                                        values[vals.Attributes[0].Value] += tmp;

                                }
                            }

                        }




                        foreach (Tortuga.Types.LifecycleStage stage in Stages)
                        {

                            double actualVal = 0;

                            if (!values.ContainsKey(stage.Name))
                            {
                                if (stage.Name == "A1-A3")
                                {
                                    if (values.ContainsKey("A1")) actualVal += values["A1"];
                                    if (values.ContainsKey("A2")) actualVal += values["A2"];
                                    if (values.ContainsKey("A3")) actualVal += values["A3"];
                                }
                            }
                            else
                            {
                                actualVal = values[stage.Name];
                            }



                            if (Name.Contains("(GWP)"))
                                mat.GlobalWarmingPotential += new UnitDouble<LCA.CO2e>(actualVal);

                            if (Name.Contains("(AP)"))
                                mat.Acidification += new UnitDouble<LCA.kgSO2>(actualVal);

                            if (Name.Contains("(ADPF)"))
                                mat.DepletionOfNonrenewbles += new UnitDouble<LCA.MJ>(actualVal);

                            if (Name.Contains("(OPD)"))
                                mat.DepletionOfOzoneLayer += new UnitDouble<LCA.kgCFC11>(actualVal);

                            if (Name.Contains("(ADPE)"))
                                mat.Eutrophication += new UnitDouble<LCA.kgPhostphate>(actualVal);

                            if (Name.Contains("(POCP)"))
                                mat.FormationTroposphericOzone += new UnitDouble<LCA.kgNOx>(actualVal);
 

                            
                        }

                    }

                }
                

            }

            

            return mat;



        }

        public static void LoadFrom(List<LifecycleStage> stages)
        {
            if (LoadedMaterials.Count == 0)
            {


                System.Net.WebClient client = new System.Net.WebClient();
                string data = client.DownloadString("http://www.oekobaudat.de/OEKOBAU.DAT/resource/processes");


                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);

                XmlNamespaceManager manager = new XmlNamespaceManager(xml.NameTable);
                manager.AddNamespace("p", "http://www.ilcd-network.org/ILCD/ServiceAPI/Process");
                manager.AddNamespace("f", "http://www.ilcd-network.org/ILCD/ServiceAPI/Flow");
                manager.AddNamespace("fp", "http://www.ilcd-network.org/ILCD/ServiceAPI/FlowProperty");
                manager.AddNamespace("u", "http://www.ilcd-network.org/ILCD/ServiceAPI/UnitGroup");
                manager.AddNamespace("l", "http://www.ilcd-network.org/ILCD/ServiceAPI/LCIAMethod");
                manager.AddNamespace("s", "http://www.ilcd-network.org/ILCD/ServiceAPI/Source");
                manager.AddNamespace("c", "http://www.ilcd-network.org/ILCD/ServiceAPI/Contact");
                manager.AddNamespace("sapi", "http://www.ilcd-network.org/ILCD/ServiceAPI");

                List<Tuple<string, string>> mats = new List<Tuple<string, string>>();


                XmlNodeList xnList = xml.SelectNodes("//sapi:name", manager);
                XmlNodeList xnList2 = xml.SelectNodes("//sapi:uuid", manager);
                for (int i = 0; i < xnList.Count; i++)
                {
                    mats.Add(new Tuple<string, string>(xnList[i].InnerText, xnList2[i].InnerText));

                    // get data from url
                    // http://www.oekobaudat.de/OEKOBAU.DAT/resource/processes/ee4fb7c2-6119-4f00-8cb6-fc74cb66fe9a?format=xml

                }


                foreach (Tuple<string, string> tuple in mats) { LoadedMaterials.Add(tuple.Item1, GetMat(tuple.Item1, tuple.Item2, stages)); }
            }
        }
    


        public static void LoadFrom(string filename, List<LifecycleStage> stages)
        {
            if (System.IO.File.Exists(filename))
            {

                LoadedMaterials.Clear();

                string[] data = System.IO.File.ReadAllLines(filename);


                for (int i = 1; i < data.Length; i++)
                {
                    string[] fields = data[i].Split(',');

                    if (fields.Length == 19)
                    {
                        UnitDouble<Types.LCA.CO2e> GWP = new UnitDouble<LCA.CO2e>(0);
                        UnitDouble<Types.LCA.kgSO2> Acidification = new UnitDouble<LCA.kgSO2>(0);
                        UnitDouble<Types.LCA.MJ> DepletionOfNonrenewbles = new UnitDouble<LCA.MJ>(0);
                        UnitDouble<Types.LCA.kgCFC11> DepletionOfOzoneLayer = new UnitDouble<LCA.kgCFC11>(0);
                        UnitDouble<Types.LCA.kgPhostphate> Eutrophication = new UnitDouble<LCA.kgPhostphate>(0);
                        UnitDouble<Types.LCA.kgNOx> FormationTroposphericOzone = new UnitDouble<LCA.kgNOx>(0);

                        foreach (LifecycleStage stage in stages)
                        {
                            GWP += new UnitDouble<Types.LCA.CO2e>(double.Parse(fields[1 + stage.Column]));
                            Acidification += new UnitDouble<Types.LCA.kgSO2>(double.Parse(fields[4 + stage.Column]));
                            DepletionOfNonrenewbles += new UnitDouble<Types.LCA.MJ>(double.Parse(fields[7 + stage.Column]));
                            DepletionOfOzoneLayer += new UnitDouble<Types.LCA.kgCFC11>(double.Parse(fields[10 + stage.Column]));
                            Eutrophication += new UnitDouble<Types.LCA.kgPhostphate>(double.Parse(fields[13 + stage.Column]));
                            FormationTroposphericOzone += new UnitDouble<Types.LCA.kgNOx>(double.Parse(fields[16 + stage.Column]));
                        }

                        Tortuga.Types.Material material = new Material()
                        {
                            Name = fields[0],

                            GlobalWarmingPotential = GWP,
                            Acidification = Acidification,
                            DepletionOfNonrenewbles = DepletionOfNonrenewbles,
                            DepletionOfOzoneLayer = DepletionOfOzoneLayer,
                            Eutrophication = Eutrophication,
                            FormationTroposphericOzone = FormationTroposphericOzone,
                        };

                        material.Stages = stages;

                        LoadedMaterials.Add(material.Name, material);
                    }
                }

            }
        }
    }

    [DataContract]
    [XmlSerializerFormat]
    [KnownType(typeof(Layer))]
    [KnownType(typeof(Material))]
    [KnownType(typeof(LifecycleStage))]
    [KnownType(typeof(UnitDouble<LCA.CO2e>))]
    [KnownType(typeof(LCA.CO2e))]
    [KnownType(typeof(UnitDouble<LCA.kgCFC11>))]
    [KnownType(typeof(LCA.kgCFC11))]
    [KnownType(typeof(UnitDouble<LCA.kgSO2>))]
    [KnownType(typeof(LCA.kgSO2))]
    [KnownType(typeof(UnitDouble<LCA.kgPhostphate>))]
    [KnownType(typeof(LCA.kgPhostphate))]
    [KnownType(typeof(UnitDouble<LCA.kgNOx>))]
    [KnownType(typeof(LCA.kgNOx))]
    [KnownType(typeof(UnitDouble<LCA.MJ>))]
    [KnownType(typeof(LCA.MJ))]
    [KnownType(typeof(LCA))]
    public class Assembly
    {
        [DataMember]
        public List<Layer> Layers;

        public override string ToString()
        {
            string result = "Tortuga Material Assembly";
            foreach (Layer layer in this.Layers) result += string.Format(", {0}", layer.Material.Name);
            return result;
        }
        
        public Assembly(){this.Layers = new List<Layer>();}

        public double Width
        {
            get
            {
                double overall = 0;
                foreach (Layer layer in this.Layers) overall += layer.Width;
                return overall;
            }
        }

        public UnitDouble<LCA.kgCFC11> DepletionOfOzoneLayer
        {
            get
            {
                UnitDouble<LCA.kgCFC11> overall = new UnitDouble<LCA.kgCFC11>(0);
                foreach (Layer layer in this.Layers) overall += layer.DepletionOfOzoneLayer;
                return overall;
            }
        }

        public UnitDouble<LCA.kgSO2> Acidification
        {
            get
            {
                UnitDouble<LCA.kgSO2> overall = new UnitDouble<LCA.kgSO2>(0);
                foreach (Layer layer in this.Layers) overall += layer.Acidification;
                return overall;
            }
        }

        public UnitDouble<LCA.kgPhostphate> Eutrophication
        {
            get
            {
                UnitDouble<LCA.kgPhostphate> overall = new UnitDouble<LCA.kgPhostphate>(0);
                foreach (Layer layer in this.Layers) overall += layer.Eutrophication;
                return overall;
            }
        }

        public UnitDouble<LCA.kgNOx> FormationTroposphericOzone
        {
            get
            {
                UnitDouble<LCA.kgNOx> overall = new UnitDouble<LCA.kgNOx>(0);
                foreach (Layer layer in this.Layers) overall += layer.FormationTroposphericOzone;
                return overall;
            }
        }

        public UnitDouble<LCA.MJ> DepletionOfNonrenewbles
        {
            get
            {
                UnitDouble<LCA.MJ> overall = new UnitDouble<LCA.MJ>(0);
                foreach (Layer layer in this.Layers) overall += layer.DepletionOfNonrenewbles;
                return overall;
            }
        }

        public UnitDouble<LCA.CO2e> GlobalWarmingPotential
        {
            get
            {
                UnitDouble<LCA.CO2e> overall = new UnitDouble<LCA.CO2e>(0);
                foreach (Layer layer in this.Layers) overall += layer.GlobalWarmingPotential;
                return overall;
            }
        }



        public static Assembly operator +(Assembly first, Assembly second)
        {
            Assembly assembly = new Assembly();

            foreach (Layer layer in first.Layers) assembly.Layers.Add(layer);
            foreach (Layer layer in second.Layers) assembly.Layers.Add(layer);

            return assembly;
        }

        public static Assembly operator *(Assembly first, double factor)
        {
            Assembly assembly = new Assembly();

            foreach (Layer layer in first.Layers)
            {
                Layer lay = new Layer(layer.Material, layer.Width * factor);                
                assembly.Layers.Add(lay);
            }
       

            return assembly;
        }

    }

    [DataContract]
    [XmlSerializerFormat]
    public class Layer
    {
        [DataMember]
        public Material Material;

        [DataMember]
        public double Width;



        public Layer(Material material, double width) { this.Width = width; this.Material = material; }

        private static double heightMax = 200;
        private static double heightMin = 20;



        public UnitDouble<LCA.kgCFC11> DepletionOfOzoneLayer
        {
            get
            {
                return new UnitDouble<LCA.kgCFC11>(this.Material.DepletionOfOzoneLayer.Value * this.Width);
            }
        }

        public UnitDouble<LCA.kgSO2> Acidification
        {
            get
            {
                return new UnitDouble<LCA.kgSO2>(this.Material.Acidification.Value * this.Width);
            }
        }

        public UnitDouble<LCA.kgPhostphate> Eutrophication
        {
            get
            {
                return new UnitDouble<LCA.kgPhostphate>(this.Material.Eutrophication.Value * this.Width);
            }
        }

        public UnitDouble<LCA.kgNOx> FormationTroposphericOzone
        {
            get
            {
                return new UnitDouble<LCA.kgNOx>(this.Material.FormationTroposphericOzone.Value * this.Width);
            }
        }

        public UnitDouble<LCA.MJ> DepletionOfNonrenewbles
        {
            get
            {
                return new UnitDouble<LCA.MJ>(this.Material.DepletionOfNonrenewbles.Value * this.Width);
            }
        }

        public UnitDouble<LCA.CO2e> GlobalWarmingPotential
        {
            get
            {
                return new UnitDouble<LCA.CO2e>(this.Material.GlobalWarmingPotential.Value * this.Width);
            }
        }

        public static ContextMenu contextMenu;

        public static void BuildContextMenu()
        {
            contextMenu = new ContextMenu();
            MenuItem moveUp = new MenuItem() { Header = "move up", Icon = new Image() { Source = Utilities.GetSourceFromBitmap(Properties.Resources.bullet_arrow_up)} }; moveUp.Click += move_Click;
            MenuItem moveDown = new MenuItem() { Header = "move down", Icon = new Image() { Source = Utilities.GetSourceFromBitmap(Properties.Resources.bullet_arrow_down) } }; moveDown.Click += move_Click;
            MenuItem moveTop = new MenuItem() { Header = "move top", Icon = new Image() { Source = Utilities.GetSourceFromBitmap(Properties.Resources.bullet_arrow_top) } }; moveTop.Click += move_Click;
            MenuItem moveBottom = new MenuItem() { Header = "move bottom", Icon = new Image() { Source = Utilities.GetSourceFromBitmap(Properties.Resources.bullet_arrow_bottom) } }; moveBottom.Click += move_Click;


            contextMenu.Items.Add(moveUp);
            contextMenu.Items.Add(moveDown);
            contextMenu.Items.Add(moveTop);
            contextMenu.Items.Add(moveBottom);

        
        }


        static void move_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = (MenuItem)sender;
            ContextMenu m = (ContextMenu)mnu.Parent;
            ListViewItem item = (ListViewItem)m.PlacementTarget;
            ListView lv = (ListView)item.Parent;
            int index = lv.SelectedIndex;

            switch (mnu.Header.ToString())
            {
                case "move up":                   
                    if (index > 0)
                    {
                        lv.Items.RemoveAt(index);
                        lv.Items.Insert(index - 1, item);
                    }
                    break;

                case "move down":
                    if (index < lv.Items.Count - 1)
                    {
                        lv.Items.RemoveAt(index);
                        lv.Items.Insert(index + 1, item);
                    }
                    break;

                case "move top":
                    lv.Items.RemoveAt(lv.SelectedIndex);
                    lv.Items.Insert(0, item);
                    break;

                case "move bottom":
                    lv.Items.RemoveAt(lv.SelectedIndex);
                    lv.Items.Add(item);
                    break;

                default: break;
            }
            

            
        }

        void inputWidth_TextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;

            double value = Layer.heightMin;

            if (double.TryParse(textBox.Text, out value))
            {
                Layer layer = (Layer)textBox.Tag;
                layer.Width = value;

                StackPanel panel = (StackPanel)textBox.Parent;


                double factor = value * 1000;
                if (factor < Layer.heightMin) panel.Height = Layer.heightMin;
                else if (factor > Layer.heightMax) panel.Height = Layer.heightMax;
                else { panel.Height = factor; }

                



            }
        }

    }


}



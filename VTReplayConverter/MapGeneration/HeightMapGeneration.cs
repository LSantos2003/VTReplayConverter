using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace VTReplayConverter
{
    //Thank you Nebriv
    public class HeightMapGeneration
    {
        public static string HeightMapLocation { get { return Path.Combine(Program.TacviewTerrainPath, "VTOL_VR_CUSTOM_MAP.raw"); } }
        public static string XmlLocation { get { return Path.Combine(Program.TacviewTerrainPath, $"CustomHeightmapList.xml"); } }
        public static void ConvertHeightMap(string heightMapPath, string configPath)
        {
            if (!Directory.Exists(Program.TacviewTerrainPath))
            {
                Console.WriteLine("Tacview Terrain Path not found, is Tacview installed?");
                return;
            }
            Bitmap heightMap = FileDecoder.DecodeImage(heightMapPath);
            ConfigNode configFile = FileDecoder.LoadFromFile(configPath);
            int mapSize = configFile.GetValue<int>("mapSize");

            Vector3 mapOffset = Vector3.zero;

            if (configFile.HasValue("mapOffset"))
            {
                mapOffset = configFile.GetValue<Vector3>("mapOffset");
            }

            SaveHeightMap(heightMap, mapSize);
            GenerateMapXML(heightMap, mapSize, mapOffset);
        }

  
        private static void SaveHeightMap(Bitmap heightMap, int mapSize)
        {

            ConvertPngToRaw(heightMap);


            string outFile = HeightMapLocation;

            Console.WriteLine("Height Map Info:");
            Console.WriteLine($"     Height: {heightMap.Height} Width: {heightMap.Width}");
            Console.WriteLine($"     Map Size {mapSize}");
            Console.WriteLine($"     Height Map Location: {outFile}");



        }

        static void ConvertPngToRaw(Bitmap bitmap)
        {
            string outFile = HeightMapLocation;

            int width = bitmap.Width;
            int height = bitmap.Height;

            using (FileStream fs = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        System.Drawing.Color pixel = bitmap.GetPixel(x, y);

                        // Assume grayscale stored in R channel, scale to 16-bit
                        ushort value16 = (ushort)(pixel.R << 8 | pixel.R); // replicate 8-bit to 16-bit
                        // Write as big-endian
                        byte highByte = (byte)((value16 >> 8) & 0xFF);
                        byte lowByte = (byte)(value16 & 0xFF);
                        fs.WriteByte(highByte);
                        fs.WriteByte(lowByte);
                    }
                }
            }
            
        }

        private static void GenerateMapXML(Bitmap texture, int mapSize, Vector3 mapOffset)
        {

            //Console.WriteLine("Generating custom XML");

            GeoLocation[] geoLocations = new GeoLocation[4];

           
            geoLocations = GenerateCoords(mapSize, mapOffset);
            
           

            int endian = 1;
            int width = texture.Width;
            int height = texture.Height;

            //TODO change values for Akutan Heightmap
            float altFactor = 0.0924f;
            float altOffset = -80f;

            string projection = "Quad";

            string xmlSavePath = XmlLocation;
            XDocument doc = new XDocument(new XElement("Resources", new XElement("CustomHeightmapList", new XElement("CustomHeightmap",
                                                new XElement("File", $"VTOL_VR_CUSTOM_MAP.raw"),
                                                new XElement("BigEndian", endian.ToString()),
                                                new XElement("Width", width.ToString()),
                                                new XElement("Height", height.ToString()),
                                                new XElement("AltitudeFactor", altFactor.ToString()),
                                                new XElement("AltitudeOffset", altOffset.ToString()),
                                                new XElement("Projection", projection.ToString()),
                                                new XElement("BottomLeft",
                                                    new XElement("Longitude", geoLocations[0].Longitude),
                                                    new XElement("Latitude", geoLocations[0].Latitude)),
                                                new XElement("BottomRight",
                                                    new XElement("Longitude", geoLocations[1].Longitude),
                                                    new XElement("Latitude", geoLocations[1].Latitude)),
                                                new XElement("TopRight",
                                                    new XElement("Longitude", geoLocations[2].Longitude),
                                                    new XElement("Latitude", geoLocations[2].Latitude)),
                                                new XElement("TopLeft",
                                                    new XElement("Longitude", geoLocations[3].Longitude),
                                                    new XElement("Latitude", geoLocations[3].Latitude))
                                                ))));

            Console.WriteLine($"Saving custom tacview custom XML to {xmlSavePath}");
            doc.Save(xmlSavePath);

        }


        private static GeoLocation[] GenerateCoords(int mapSize, Vector3 mapOffset)
        {
            GeoLocation[] geoLocations = new GeoLocation[4];


            GeoLocation bottomLeft = new GeoLocation();
            bottomLeft.Longitude = ACMIUtils.WorldPositionToGPSCoords(-mapOffset).x;
            bottomLeft.Latitude = ACMIUtils.WorldPositionToGPSCoords(-mapOffset).y;

            GeoLocation bottomRight = new GeoLocation();
            bottomRight.Longitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(0, 0, mapSize) - mapOffset).x;
            bottomRight.Latitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(0, 0, mapSize) - mapOffset).y;

            GeoLocation topLeft = new GeoLocation();
            topLeft.Longitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, 0) - mapOffset).x;
            topLeft.Latitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, 0) - mapOffset).y;

            GeoLocation topRight = new GeoLocation();
            topRight.Longitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, mapSize) - mapOffset).x;
            topRight.Latitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, mapSize) - mapOffset).y;

            geoLocations[0] = bottomLeft;
            geoLocations[1] = bottomRight;
            geoLocations[2] = topRight;
            geoLocations[3] = topLeft;




            return geoLocations;
        }

        public struct GeoLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public override string ToString()
            {
                return ($"{Latitude}, {Longitude}");
            }
        }

    }
    
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmearTracer
{
    public class NeuronNetwork
    {
        public List<Neuron[]> NeuronsMap { get; set; }

        public NeuronNetwork(int width, int height, List<Pixel> inputData)
        {            
            NeuronsMap = new List<Neuron[]>();
            if (width * height > 0)
            {
                double[][] samples = SamplesForNetwork(inputData, width, height);
                int counter = 0;
                for (int k = 0; k < width; k++)
                {
                    Neuron[] arrayNeurons = new Neuron[height];
                    for (int i = 0; i < height; i++)
                    {
                        arrayNeurons[i] = new Neuron(samples[counter++]);
                    }
                    NeuronsMap.Add(arrayNeurons);
                }
            }

        }

        private double[][] SamplesForNetwork(List<Pixel> data, int widthMap, int heightMap)
        {
            double[][] samplesData = new double[widthMap*heightMap][];

            int step = data.Count / widthMap / heightMap;
            for (int i = 0; i < samplesData.Length; i++)
            {
                samplesData[i] = data[i * step].Coordinates;
            }
            return samplesData;
        }
    }
}

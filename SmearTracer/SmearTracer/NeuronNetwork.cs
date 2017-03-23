using System.Collections.Generic;

namespace SmearTracer
{
    public class NeuronNetwork
    {
        public List<Neuron> NeuronsList { get; set; }

        public NeuronNetwork()
        {
            
        }

        public NeuronNetwork(int length, List<Pixel> inputData)
        {
            NeuronsList = new List<Neuron>();
            double[][] samples = SamplesForNetwork(inputData, length);
            int counter = 0;
            for (int k = 0; k < length; k++)
            {
                Neuron neuron = new Neuron(samples[counter++]);
                NeuronsList.Add(neuron);
            }
        }

        private double[][] SamplesForNetwork(List<Pixel> data, int length)
        {
            double[][] samplesData = new double[length][];
            int step = data.Count / length;

            for (int i = 0; i < samplesData.Length; i++)
            {
                samplesData[i] = data[i * step].Coordinates;
            }
            return samplesData;
        }
    }
}

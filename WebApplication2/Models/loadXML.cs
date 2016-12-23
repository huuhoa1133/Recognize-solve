using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

using System.Linq;

using System.Web.Mvc;


namespace WebApplication2.Models
{
    public class LoadXMLModel
    {
        List<double> w12;
        List<double> bias2;
        List<double> w23;
        List<double> bias3;
        XmlDocument doc;

        int numInput ;
        int numHidden;
        int numOutput;
        public LoadXMLModel()
        {
            doc = new XmlDocument();
            numInput = 784;
            numHidden = 200;
            numOutput = 14;
            bias2 = new List<double>();
            w12 = new List<double>();
            bias3 = new List<double>();
            w23 = new List<double>();
        }
        private string xPathValue(string xPath)
        {

            XmlNode node = doc.SelectSingleNode(xPath);
            if (node == null)
            {
                throw new ArgumentException("Cannot find spacified node", xPath);
            }
            return node.InnerText;
        }
        public List<double> GetW12() { return w12; }
        public List<double> GetW23() { return w23; }
        public List<double> GetBias2() { return bias2; }
        public List<double> GetBias3() { return bias3; }
        public void LoadXml(String FilePath)
        {

            doc.Load(FilePath);
            double value;

            string BasePath = "NeuralNetwork/Weight/layer[@Index='0']/";
            //bias 2
            

            for (int i = 0; i < numHidden; i++)
            {
                string Nodepath = "Node[@Index='" + i.ToString() + "']/@Bias";
                double.TryParse(xPathValue(BasePath + Nodepath), out value);

                bias2.Add(value);
            }
            //w12
            
            for (int j = 0; j < numHidden; j++)
            {
                for (int k = 0; k < numInput; k++)
                {
                    string Nodepath = "Node[@Index='" + j.ToString() + "']/Axon[@Index='" + k.ToString() + "']";
                    double.TryParse(xPathValue(BasePath + Nodepath), out value);
                    w12.Add(value);
                }

            }

            
            //bias3
            
            BasePath = "NeuralNetwork/Weight/layer[@Index='1']/";
            for (int i = 0; i < numOutput; i++)
            {
                string Nodepath = "Node[@Index='" + i.ToString() + "']/@Bias";
                double.TryParse(xPathValue(BasePath + Nodepath), out value);

                bias3.Add(value);
            }
            //w23
            
            for (int j = 0; j < numOutput; j++)
            {
                for (int k = 0; k < numHidden; k++)
                {
                    string Nodepath = "Node[@Index='" + j.ToString() + "']/Axon[@Index='" + k.ToString() + "']";
                    double.TryParse(xPathValue(BasePath + Nodepath), out value);

                    w23.Add(value);
                }

            }


        }

    }
    public class Backpropagation
    {
        String Name = "Default";
        XmlDocument doc;
        Random rand = new Random();

        private int numInput;
        private int numHidden;
        private int numOutput;

        private double[] inputs;
        private double[][] ihWeights; // input-to-hidden
        private double[] hBiases;
        private double[] hSums;
        private double[] hOutputs;

        private double[][] hoWeights;  // hidden-to-output
        private double[] oBiases;
        private double[] oSums;
        private double[] outputs;

        //private string hActivation; // "log-sigmoid" or "tanh"
        //private string oActivation; // "log-sigmoid" or "tanh"

        private double[] oGrads; // output gradients for back-propagation
        private double[] hGrads; // hidden gradients for back-propagation

        private double[][] ihPrevWeightsDelta;  // for momentum with back-propagation
        private double[] hPrevBiasesDelta;
        private double[][] hoPrevWeightsDelta;
        private double[] oPrevBiasesDelta;

        public Backpropagation(int numInput, int numHidden, int numOutput)
        {
            this.numInput = numInput;
            this.numHidden = numHidden;
            this.numOutput = numOutput;

            Init();


            InitWeight();
        }

        public Backpropagation(String FilePath)
        {
            Load(FilePath);

        }
        public Backpropagation() { }
        public void getWeightBias(out double[][] w12, out double[] bias2, out double[][] w23, out double[] bias3,out int num0,out int num1,out int num2)
        {
            w12 = new double[numHidden][];
            for (int i = 0; i < numHidden; i++)
            {
                w12[i] = new double[numInput];
            }
            bias2 = new double[numHidden];

            w23 = new double[numOutput][];
            for (int i = 0; i < numOutput; i++)
            {
                w23[i] = new double[numHidden];
            }
            bias3 = new double[numOutput];

            //coppy w12
            for (int i = 0; i < numHidden; i++)
            {
                for (int j = 0; j < numInput; j++)
                {
                    w12[i][j] = ihWeights[j][i];
                }
                bias2[i] = hBiases[i];
            }
            //coppy w23
            for (int i = 0; i < numOutput; i++)
            {
                for (int j = 0; j < numHidden; j++)
                {
                    w23[i][j] = hoWeights[j][i];
                }
                bias3[i] = oBiases[i];
            }
            num0 = numInput;
            num1 = numHidden;
            num2 = numOutput;

        }
        private void Init()
        {
            inputs = new double[numInput];
            ihWeights = MakeMatrix(numInput, numHidden);
            hBiases = new double[numHidden];
            hSums = new double[numHidden];

            hOutputs = new double[numHidden];
            hoWeights = MakeMatrix(numHidden, numOutput);
            oBiases = new double[numOutput];
            oSums = new double[numOutput];
            outputs = new double[numOutput];

            oGrads = new double[numOutput];
            hGrads = new double[numHidden];

            ihPrevWeightsDelta = MakeMatrix(numInput, numHidden);
            hPrevBiasesDelta = new double[numHidden];
            hoPrevWeightsDelta = MakeMatrix(numHidden, numOutput);
            oPrevBiasesDelta = new double[numOutput];
        }
        private void InitWeight()
        {
            for (int i = 0; i < numInput; i++)
            {
                for (int j = 0; j < numHidden; j++)
                {
                    ihWeights[i][j] = rand.NextDouble() * 0.01;
                }
            }
            for (int i = 0; i < numHidden; i++)
            {
                for (int j = 0; j < numOutput; j++)
                {
                    hoWeights[i][j] = rand.NextDouble() * 0.01;
                }
                hBiases[i] = rand.NextDouble() * 0.01;
            }
            for (int i = 0; i < numOutput; i++)
            {
                oBiases[i] = rand.NextDouble() * 0.01;
            }


        }
        public void SetWeights(double[] weights)
        {
            // assumes weights[] has order: input-to-hidden wts, hidden biases, hidden-to-output wts, output biases
            int numWeights = (numInput * numHidden) + (numHidden * numOutput) + numHidden + numOutput;
            if (weights.Length != numWeights)
                throw new Exception("The weights array length: " + weights.Length +
                  " does not match the total number of weights and biases: " + numWeights);

            int k = 0; // points into weights param

            for (int i = 0; i < numInput; ++i)
                for (int j = 0; j < numHidden; ++j)
                    ihWeights[i][j] = weights[k++];

            for (int i = 0; i < numHidden; ++i)
                hBiases[i] = weights[k++];

            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    hoWeights[i][j] = weights[k++];

            for (int i = 0; i < numOutput; ++i)
                oBiases[i] = weights[k++];
        }
        public double[] ComputeOutputs(double[] xValues)
        {
            if (xValues.Length != numInput)
                throw new Exception("Inputs array length " + inputs.Length + " does not match NN numInput value " + numInput);

            for (int i = 0; i < numHidden; ++i)
                hSums[i] = 0.0;
            for (int i = 0; i < numOutput; ++i)
                oSums[i] = 0.0;

            for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
                this.inputs[i] = xValues[i];

            for (int j = 0; j < numHidden; ++j)  // compute hidden layer weighted sums
                for (int i = 0; i < numInput; ++i)
                    hSums[j] += this.inputs[i] * ihWeights[i][j];

            for (int i = 0; i < numHidden; ++i)  // add biases to hidden sums
                hSums[i] += hBiases[i];

            for (int i = 0; i < numHidden; ++i)   // apply tanh activation
                hOutputs[i] = HyperTanFunction(hSums[i]);

            for (int j = 0; j < numOutput; ++j)   // compute output layer weighted sums
                for (int i = 0; i < numHidden; ++i)
                    oSums[j] += hOutputs[i] * hoWeights[i][j];

            for (int i = 0; i < numOutput; ++i)  // add biases to output sums
                oSums[i] += oBiases[i];

            for (int i = 0; i < numOutput; ++i)   // apply log-sigmoid activation
                this.outputs[i] = SigmoidFunction(oSums[i]);

            double[] result = new double[numOutput]; // for convenience when calling method
            this.outputs.CopyTo(result, 0);
            return result;
        } // ComputeOutputs
        private static double SigmoidFunction(double x)
        {
            if (x < -45.0) return 0.0;
            else if (x > 45.0) return 1.0;
            else return 1.0 / (1.0 + Math.Exp(-x));
        }
        private static double HyperTanFunction(double x)
        {
            if (x < -45.0) return -1.0;
            else if (x > 45.0) return 1.0;
            else return Math.Tanh(x);
        }
        public void UpdateWeights(double[] tValues, double learn, double mom) // back-propagation
        {
            // assumes that SetWeights and ComputeOutputs have been called and so inputs and outputs have values
            if (tValues.Length != numOutput)
                throw new Exception("target values not same Length as output in UpdateWeights");

            // 1. compute output gradients. assumes log-sigmoid!
            for (int i = 0; i < oGrads.Length; ++i)
            {
                double derivative = (1 - outputs[i]) * outputs[i]; // derivative of log-sigmoid is y(1-y)
                oGrads[i] = derivative * (tValues[i] - outputs[i]); // oGrad = (1 - O)(O) * (T-O)
            }

            // 2. compute hidden gradients. assumes tanh!
            for (int i = 0; i < hGrads.Length; ++i)
            {
                double derivative = (1 - hOutputs[i]) * (1 + hOutputs[i]); // derivative of tanh is (1-y)(1+y)
                double sum = 0.0;
                for (int j = 0; j < numOutput; ++j) // each hidden delta is the sum of numOutput terms
                    sum += oGrads[j] * hoWeights[i][j]; // each downstream gradient * outgoing weight
                hGrads[i] = derivative * sum; // hGrad = (1-O)(1+O) * E(oGrads*oWts)
            }

            // 5. update hidden to output weights
            for (int i = 0; i < hoWeights.Length; ++i)  // 0..3 (4)
            {
                for (int j = 0; j < hoWeights[0].Length; ++j) // 0..1 (2)
                {
                    double delta = learn * oGrads[j] * hOutputs[i];  // hOutputs are inputs to next layer
                    hoWeights[i][j] += delta;
                    hoWeights[i][j] += mom * hoPrevWeightsDelta[i][j];
                    hoPrevWeightsDelta[i][j] = delta;
                }
            }

            // 6. update hidden to output biases
            for (int i = 0; i < oBiases.Length; ++i)
            {
                //double delta = learn * oGrads[i] * oBiases[i];
                double delta = learn * oGrads[i] * 1.0;
                oBiases[i] += delta;
                oBiases[i] += mom * oPrevBiasesDelta[i];
                oPrevBiasesDelta[i] = delta;
            }

            // 3. update input to hidden weights (gradients must be computed right-to-left but weights can be updated in any order)
            for (int i = 0; i < ihWeights.Length; ++i) // 0..2 (3)
            {
                for (int j = 0; j < ihWeights[0].Length; ++j) // 0..3 (4)
                {
                    double delta = learn * hGrads[j] * inputs[i]; // compute the new delta = "eta * hGrad * input"
                    ihWeights[i][j] += delta; // update
                    ihWeights[i][j] += mom * ihPrevWeightsDelta[i][j]; // add momentum using previous delta. on first pass old value will be 0.0 but that's OK.
                    ihPrevWeightsDelta[i][j] = delta; // save the delta for next time
                }
            }

            // 4. update hidden biases
            for (int i = 0; i < hBiases.Length; ++i)
            {
                //double delta = learn * hGrads[i] * hBiases[i];
                double delta = learn * hGrads[i] * 1.0; // the 1.0 is the constant input for any bias; could leave out
                hBiases[i] += delta;
                hBiases[i] += mom * hPrevBiasesDelta[i];
                hPrevBiasesDelta[i] = delta; // save delta
            }


        } // UpdateWeights
        private static double[][] MakeMatrix(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }
        public static double Error(double[] tValues, double[] yValues)
        {
            double sum = 0.0;
            for (int i = 0; i < tValues.Length; ++i)
                sum += (tValues[i] - yValues[i]) * (tValues[i] - yValues[i]);
            return Math.Sqrt(sum);
        }
        public void Save(String FilePath)
        {

            if (FilePath == null)
            {
                return;
            }
            XmlWriter writer = XmlWriter.Create(FilePath);
            //begin document

            writer.WriteStartElement("NeuralNetwork");
            writer.WriteAttributeString("Type", "BackPropagation");

            //parameter elements
            writer.WriteStartElement("Parameter");
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("inputSize", numInput.ToString());
            writer.WriteElementString("LayerCount", "2");

            //layer Size

            writer.WriteStartElement("Layers");

            writer.WriteStartElement("Layer");
            writer.WriteAttributeString("index", "0");
            writer.WriteAttributeString("Size", numHidden.ToString());
            writer.WriteAttributeString("type", "tanh");
            writer.WriteEndElement();

            writer.WriteStartElement("Layer");
            writer.WriteAttributeString("index", "1");
            writer.WriteAttributeString("Size", numOutput.ToString());
            writer.WriteAttributeString("type", "Sigmoid");
            writer.WriteEndElement();

            writer.WriteEndElement();//player
            writer.WriteEndElement();//parameters

            //weight and bias
            writer.WriteStartElement("Weight");

            //input to hidden
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("Index", "0");

            for (int j = 0; j < numHidden; j++)
            {
                writer.WriteStartElement("Node");
                writer.WriteAttributeString("Index", j.ToString());
                writer.WriteAttributeString("Bias", hBiases[j].ToString());
                for (int i = 0; i < numInput; i++)
                {
                    writer.WriteStartElement("Axon");
                    writer.WriteAttributeString("Index", i.ToString());
                    writer.WriteString(ihWeights[i][j].ToString());
                    writer.WriteEndElement();//Axon
                }
                writer.WriteEndElement();//end Node
            }
            writer.WriteEndElement();//layer
            //hidden to output
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("Index", "1");

            for (int j = 0; j < numOutput; j++)
            {
                writer.WriteStartElement("Node");
                writer.WriteAttributeString("Index", j.ToString());
                writer.WriteAttributeString("Bias", oBiases[j].ToString());
                for (int i = 0; i < numHidden; i++)
                {
                    writer.WriteStartElement("Axon");
                    writer.WriteAttributeString("Index", i.ToString());
                    writer.WriteString(hoWeights[i][j].ToString());
                    writer.WriteEndElement();//Axon
                }
                writer.WriteEndElement();//end Node
            }


            writer.WriteEndElement();//layer
            writer.WriteEndElement();//weight



            writer.WriteEndElement();//neural network
            writer.Flush();
            writer.Close();

        }

        private void Load(String FilePath)
        {
            if (FilePath == null)
            {
                return;
            }

            
            doc = new XmlDocument();
            doc.Load(FilePath);
            string BasePath = "";
            double value;
            //load from xml
            BasePath = "NeuralNetwork/Parameter/";
            int.TryParse(xPathValue(BasePath + "inputSize"), out numInput);

            BasePath = "NeuralNetwork/Parameter/Layers/Layer";
            int.TryParse(xPathValue(BasePath + "[@index='0']/@Size"), out numHidden);
            int.TryParse(xPathValue(BasePath + "[@index='1']/@Size"), out numOutput);

            Init();

            //init weight & bias input to hidden
            BasePath = "NeuralNetwork/Weight/layer[@Index='0']/";
            for (int i = 0; i < numHidden; i++)
            {
                string Nodepath = "Node[@Index='" + i.ToString() + "']/@Bias";
                double.TryParse(xPathValue(BasePath + Nodepath), out value);

                //hBiases[i] = value;

                //DatabaseRecog data = new DatabaseRecog();
                //data.value = value;
                //data.type = 2;
                //new DatabaseDao().inset(data);

            }
            //for (int j = 0; j < numInput; j++)
            //{
            //    for (int k = 0; k < numHidden; k++)
            //    {
            //        string Nodepath = "Node[@Index='" + k.ToString() + "']/Axon[@Index='" + j.ToString() + "']";
            //        double.TryParse(xPathValue(BasePath + Nodepath), out value);
            //        //ihWeights[j][k] = value;
            //        DatabaseRecog data = new DatabaseRecog();
            //        data.value = value;
            //        data.type = 1;
            //        new DatabaseDao().inset(data);
            //    }

            //}
            for (int j = 0; j < numHidden; j++)
            {
                for (int k = 0; k < numInput; k++)
                {
                    string Nodepath = "Node[@Index='" + j.ToString() + "']/Axon[@Index='" + k.ToString() + "']";
                    double.TryParse(xPathValue(BasePath + Nodepath), out value);
                    //ihWeights[j][k] = value;
                    //DatabaseRecog data = new DatabaseRecog();
                    //data.value = value;
                    //data.type = 1;
                    //new DatabaseDao().inset(data);
                }

            }
            
            //init weight & bias hidden to output
            BasePath = "NeuralNetwork/Weight/layer[@Index='1']/";
            for (int i = 0; i < numOutput; i++)
            {
                string Nodepath = "Node[@Index='" + i.ToString() + "']/@Bias";
                double.TryParse(xPathValue(BasePath + Nodepath), out value);

                //oBiases[i] = value;
                
                //DatabaseRecog data = new DatabaseRecog();
                
                //data.value = value;
                //data.type = 4;
                //new DatabaseDao().inset(data);
            }

            //for (int j = 0; j < numHidden; j++)
            //{
            //    for (int k = 0; k < numOutput; k++)
            //    {
            //        string Nodepath = "Node[@Index='" + k.ToString() + "']/Axon[@Index='" + j.ToString() + "']";
            //        double.TryParse(xPathValue(BasePath + Nodepath), out value);
            //        //hoWeights[j][k] = value;

            //        DatabaseRecog data = new DatabaseRecog();
            //        data.value = value;
            //        data.type = 3;
            //        new DatabaseDao().inset(data);
            //    }

            //}
            for (int j = 0; j < numOutput; j++)
            {
                for (int k = 0; k < numHidden; k++)
                {
                    string Nodepath = "Node[@Index='" + j.ToString() + "']/Axon[@Index='" + k.ToString() + "']";
                    double.TryParse(xPathValue(BasePath + Nodepath), out value);
                    //hoWeights[j][k] = value;

                    //DatabaseRecog data = new DatabaseRecog();
                    //data.value = value;
                    //data.type = 3;
                    //new DatabaseDao().inset(data);
                }

            }
            

        }
        private string xPathValue(string xPath)
        {

            XmlNode node = doc.SelectSingleNode(xPath);
            if (node == null)
            {
                throw new ArgumentException("Cannot find spacified node", xPath);
            }
            return node.InnerText;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using aicontroller;
using System.Linq;
using System;
using UnityEngine.UI;
using TMPro;

namespace GNN_AI
{

    public class GNN
    {
        
        public class WeightsInfo
        {
            // hidden layer weights
            public double[,] weights1;

            // output layer weights
            public double[,] weights2;

            // score
            public float fitness;

            public WeightsInfo(double[,] weights1, double[,] weights2, float fitness)
            {
                this.weights1 = weights1;
                this.weights2 = weights2;
                this.fitness = fitness;
            }

            public WeightsInfo()
            {
                this.fitness = 0;
                this.weights1 = new double[inputSize, hiddenSize];
                this.weights2 = new double[hiddenSize, outputSize];
            }
        }

        public AIController gameController;
        System.Random r = new System.Random();
        static int inputSize, hiddenSize, outputSize;
        double[,] input, output;
        List<WeightsInfo> weightsList = new List<WeightsInfo>();
        List<WeightsInfo> nextWeightsList = new List<WeightsInfo>();
       
        int crtIndex = 0; // Tror det har med parrallelism

        public GNN(AIController gameController)
        {
            this.gameController = gameController;
        }
        public void createFirstGeneration()
        {
            inputSize = 4;
            hiddenSize = 3;//välj någon senare
            outputSize = 3;

            for (int k = 0; k < POPULATION_SIZE; k++)
            {
                double[,] _weights1 = new double[inputSize, hiddenSize];

                for (int i = 0; i < _weights1.GetLength(0); i++)
                    for (int j = 0; j < _weights1.GetLength(1); j++)
                        _weights1[i, j] = r.NextDouble() * 2 - 1;


                double[,] _weights2 = new double[hiddenSize, outputSize];

                for (int i = 0; i < _weights2.GetLength(0); i++)
                    for (int j = 0; j < _weights2.GetLength(1); j++)
                        _weights2[i, j] = r.NextDouble() * 2 - 1;

                weightsList.Add(new WeightsInfo(_weights1, _weights2, 0));
            }
        }

        

        float xDistFirstP, yDistFirstP, xDistNextP, yDistNextP = 0;
        // called by the game's update() method
        // returns: true if the bird should jump, false otherwise
        public double[,] runForward(TextMeshProUGUI t1, TextMeshProUGUI t2)
        {

            Vector3[] vecArray = gameController.getRelativePos();
            xDistFirstP = vecArray[0].x;
            yDistFirstP = vecArray[0].y;
            xDistNextP = vecArray[1].x;
            yDistNextP = vecArray[1].y;

            // the inputs for the neural network
            input = new double[1, inputSize];
            Camera cam = gameController.camera;
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0,  cam.nearClipPlane));
            Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
            float camWidth = topRight.x - bottomLeft.x;
            float camHeight = topRight.y - bottomLeft.y;
            input[0, 0] = (camWidth/2 + xDistFirstP)/camWidth;//normalized coordinates
            input[0, 1] = (camHeight / 2 + yDistFirstP) / camHeight; 
            input[0, 2] = (camWidth / 2 + xDistNextP) / camWidth;
            input[0, 3] = (camHeight / 2 + yDistNextP) / camHeight;
            // computing the inputs & outputs for the hidden layer
            double[,] hiddenInputs = multiplyArrays(input, weightsList[crtIndex].weights1);//crtIndex used in src code
            double[,] hiddenOutputs = applySigmoid(hiddenInputs);
            t1.text = String.Join(" ", weightsList[crtIndex].weights1.Cast<double>());

            t2.text = String.Join(" ", weightsList[crtIndex].weights2.Cast<double>());

            // then the final output
            output = applySigmoid(multiplyArrays(hiddenOutputs, weightsList[crtIndex].weights2));
            return output;//[0,0] in our case??
        }
        void encode(WeightsInfo weightsInfo, List<double> gene)
        {
            for (int i = 0; i < weightsInfo.weights1.GetLength(0); i++)
                for (int j = 0; j < weightsInfo.weights1.GetLength(1); j++)
                {
                    gene.Add(weightsInfo.weights1[i, j]);
                }

            for (int i = 0; i < weightsInfo.weights2.GetLength(0); i++)
                for (int j = 0; j < weightsInfo.weights2.GetLength(1); j++)
                {
                    gene.Add(weightsInfo.weights2[i, j]);
                }
        }

        void decode(WeightsInfo weightsInfo, List<double> gene)
        {
            for (int i = 0; i < weightsInfo.weights1.GetLength(0); i++)
                for (int j = 0; j < weightsInfo.weights1.GetLength(1); j++)
                {
                    weightsInfo.weights1[i, j] = gene[0];
                    gene.RemoveAt(0);
                }

            for (int i = 0; i < weightsInfo.weights2.GetLength(0); i++)
                for (int j = 0; j < weightsInfo.weights2.GetLength(1); j++)
                {
                    weightsInfo.weights2[i, j] = gene[0];
                    gene.RemoveAt(0);
                }
        }
        void crossover(List<double> gene1, List<double> gene2)
        {
            if (r.NextDouble() > CROSSOVER_RATE)
                return;


            List<double> descendant1 = new List<double>();
            List<double> descendant2 = new List<double>();

            // mixing the genes using the arithmetic mean
            for (int i = 0; i < gene1.Count; i++)
            {
                descendant1.Add((gene1[i] + gene2[i]) / 2.0);
                descendant2.Add((gene1[i] + gene2[i]) / 2.0);
            }


            // decoding the result back to the "weights-format"
            WeightsInfo weightsInfo1 = new WeightsInfo();
            decode(weightsInfo1, descendant1);
            nextWeightsList.Add(weightsInfo1);


            WeightsInfo weightsInfo2 = new WeightsInfo();
            decode(weightsInfo2, descendant2);
            nextWeightsList.Add(weightsInfo2);

        }

        bool mutate(List<double> gene)
        {
            bool mutated = false;

            for (int i = 0; i < gene.Count; i++)
            {
                if (r.NextDouble() < MUTATION_RATE)
                {
                    gene[i] += (r.NextDouble() * 2 - 1);
                    mutated = true;
                }
            }

            return mutated;
        }

        // selection function for crossover (picks the better one from 2 random candidates)
        WeightsInfo select()
        {
            int i1 = 0;
            int i2 = 0;

            while (i1 == i2)
            {
                i1 = r.Next(0, weightsList.Count / 3);
                i2 = r.Next(0, weightsList.Count / 3);
            }

            if (weightsList[i1].fitness > weightsList[i2].fitness)
                return weightsList[i1];
            else
                return weightsList[i2];
        }

        double CROSSOVER_RATE = 0.8;
        double MUTATION_RATE = 0.05;
        int POPULATION_SIZE = 25;

        float averageFitness = 0;
        float maxFitness = 0;
        int generation = 0;

        public void breedNetworks()
        {
            weightsList[crtIndex].fitness = gameController.player.transform.position.x;
            averageFitness += weightsList[crtIndex].fitness;
            maxFitness = maxFitness > weightsList[crtIndex].fitness ? maxFitness : weightsList[crtIndex].fitness;
            if (crtIndex + 1 < weightsList.Count)
                crtIndex++;
            else
            {
                crtIndex = 0;
                generation++;
                Debug.Log("GEN: " + generation + " | AVG: " + averageFitness / (float)POPULATION_SIZE + " | MAX: " + maxFitness);
                averageFitness = 0;
                maxFitness = 0;
                

                weightsList = weightsList.OrderByDescending(wi => wi.fitness).ToList();
                // starting with a large mutation rate so there's will be more solutions to choose from
                if (weightsList[0].fitness < 2)
                    MUTATION_RATE = 0.9;
                else
                    MUTATION_RATE = 0.05;

                int iterations = 0;
                // creating a new generation 
                while (nextWeightsList.Count < POPULATION_SIZE && iterations < 50)
                {
                    iterations++;
                    WeightsInfo w1 = select();
                    WeightsInfo w2 = select();


                    while (w1 == w2)
                    {
                        w1 = select();
                        w2 = select();
                    }

                    List<double> gene1 = new List<double>();
                    List<double> gene2 = new List<double>();

                    encode(w1, gene1);
                    encode(w2, gene2);


                    crossover(gene1, gene2);

                    if (mutate(gene1))
                        w1 = new WeightsInfo();

                    if (mutate(gene2))
                        w2 = new WeightsInfo();

                    decode(w1, gene1);
                    decode(w2, gene2);

                    if (!nextWeightsList.Contains(w1))
                        nextWeightsList.Add(w1);

                    if (!nextWeightsList.Contains(w2))
                        nextWeightsList.Add(w2);
                }
                

                weightsList.Clear();
                nextWeightsList = nextWeightsList.OrderByDescending(wi => wi.fitness).ToList();


                weightsList.AddRange(nextWeightsList);

                nextWeightsList.Clear();

            }
        }


        //math functions
        double[,] applySigmoid(double[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    array[i, j] = sigmoid(array[i, j]);

            return array;
        }       


        double sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        double[,] multiplyArrays(double[,] a1, double[,] a2)
        {
            double[,] a3 = new double[a1.GetLength(0), a2.GetLength(1)];

            for (int i = 0; i < a3.GetLength(0); i++)
                for (int j = 0; j < a3.GetLength(1); j++)
                {
                    a3[i, j] = 0;
                    for (int k = 0; k < a1.GetLength(1); k++)
                        a3[i, j] = a3[i, j] + a1[i, k] * a2[k, j];
                }
            return a3;
        }
    }
}
    

﻿using System;
using System.Linq;
using System.Text;


namespace NSGA_II
{
    public class Genotype     
    {
        Random random = new Random();
        public string gene;


        public Genotype(int nBytes = 16)
        {
            for (int i = 0; i < nBytes; i++)
                gene += random.NextDouble() < 0.5 ? "1" : "0";
        }


        // Mutate: Gives genes a small probability of mutation
        public void Mutate()
        {
            StringBuilder newGene = new StringBuilder(gene, gene.Length);
            double probabilityMutation = 1.0 / gene.Length;

            for (int i = 0; i < gene.Length; i++)
                if (random.NextDouble() < probabilityMutation)
                    newGene[i] = (newGene[i] == '1') ? '0' : '1';

            gene = newGene.ToString();
        }


        // Decode: Remaps the gene's binary code (string) to a number between 0 and 1
        public double Decode(string gene)
        {
            int sum = 0;
            for (int i = gene.Length - 1; i >= 0; i--)
                sum += ((gene[i] == '1') ? 1 : 0) * (int)Math.Pow(2.0, gene.Length - 1 - i);

            double max = Enumerable.Range(0, gene.Length).Sum(i => Math.Pow(2.0, i));
            double decodedGene = sum / max;
            return decodedGene;
        }
    }
}

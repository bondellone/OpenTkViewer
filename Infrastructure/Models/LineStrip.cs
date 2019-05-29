using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Models.Enums;

namespace Infrastructure.Models
{
    public abstract class LineStrip<T, TK> where TK : class
    {
        public readonly List<TK> Edges;

        public LinkedList<LineStripElement<T, TK>> Elements { get; private set; }

        public ContourType Type { get; set; }

        public T Start => Elements.First.Value.Position;

        public T End => Elements.Last.Value.Position;

        protected LineStrip()
        {
            Type = ContourType.Open;
            Edges = new List<TK>();
            Elements = new LinkedList<LineStripElement<T, TK>>();
        }

        public void AppendLineStripToStart(LineStrip<T, TK> strip, TK edge)
        {
            AddStripEdges(strip, edge);
            Elements.First.Value.Elements.Add(edge);
            strip.Elements.First.Value.Elements.Add(edge);
            foreach (var vertex in strip.Elements)
                AppendToStart(vertex);
        }

        public void AppendLineStripToStartReverse(LineStrip<T, TK> strip, TK edge)
        {
            AddStripEdges(strip, edge);
            var current = strip.Elements.Last;
            current.Value.Elements.Add(edge);
            Elements.First.Value.Elements.Add(edge);
            while (current != null)
            {
                AppendToStart(current.Value);
                current = current.Previous;
            }
        }

        public void AppendToStart(T vertex, TK edge = null)
        {
            var newElement = new LineStripElement<T, TK>(vertex, edge);
            AppendToStart(newElement, edge);
        }

        private void AppendToStart(LineStripElement<T, TK> newElement, TK edge = null)
        {
            if (edge != null)
                Edges.Add(edge);

            if (InternalEquals(Start, newElement.Position))
                return;

            var firstVertex = Elements.First;
            if (edge != null)
            {
                if (firstVertex.Value.Elements.Count == 2)
                    throw new Exception();

                firstVertex.Value.Elements.Add(edge);
            }

            // ReSharper disable once PossibleNullReferenceException
            if (Collinear(
                newElement.Position,
                firstVertex.Value.Position,
                firstVertex.Next.Value.Position))
            {
                firstVertex.Value = newElement;
            }
            else
            {
                Elements.AddFirst(newElement);
            }
        }

        public void AppendLineStripToEnd(LineStrip<T, TK> strip, TK edge)
        {
            AddStripEdges(strip, edge);
            Elements.Last.Value.Elements.Add(edge);
            strip.Elements.First.Value.Elements.Add(edge);
            foreach (var vertex in strip.Elements)
                AppendToEnd(vertex);
        }

        public void AppendLineStripToEndReverse(LineStrip<T, TK> strip, TK edge = null)
        {
            AddStripEdges(strip, edge);
            var current = strip.Elements.Last;
            current.Value.Elements.Add(edge);
            Elements.Last.Value.Elements.Add(edge);
            while (current != null)
            {
                AppendToEnd(current.Value);
                current = current.Previous;
            }
        }

        public void AppendToEnd(T vertex, TK edge = null)
        {
            var newElement = new LineStripElement<T, TK>(vertex, edge);
            AppendToEnd(newElement, edge);
        }

        public void AppendToEnd(LineStripElement<T, TK> newElement, TK edge = null)
        {
            if (edge != null)
                Edges.Add(edge);

            if (InternalEquals(End, newElement.Position))
                return;

            var lastVertex = Elements.Last;

            if (edge != null)
            {
                if (lastVertex.Value.Elements.Count == 2)
                    throw new Exception();

                lastVertex.Value.Elements.Add(edge);
            }

            // ReSharper disable once PossibleNullReferenceException
            if (Collinear(
                lastVertex.Previous.Value.Position,
                lastVertex.Value.Position,
                newElement.Position))
            {
                lastVertex.Value = newElement;
            }
            else
            {
                Elements.AddLast(newElement);
            }
        }

        private void AddStripEdges(LineStrip<T, TK> strip, TK newEdge)
        {
            Edges.Add(newEdge);
            foreach (var edge in strip.Edges)
                Edges.Add(edge);
        }

        protected abstract bool InternalEquals(T first, T second);

        protected abstract bool Collinear(T first, T second, T third);

        public void CloseLineStrip(TK newEdge)
        {
            Elements.First.Value.Elements.Add(newEdge);
            Elements.Last.Value.Elements.Add(newEdge);
            Edges.Add(newEdge);
        }

        public void Reverse()
        {
            Elements = new LinkedList<LineStripElement<T, TK>>(Elements.Reverse());
        }
    }
}
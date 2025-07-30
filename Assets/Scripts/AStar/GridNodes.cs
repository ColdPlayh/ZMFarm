using UnityEngine;

namespace LYFarm.AStarN
{
    public class GridNodes 
    {
        private int width;
        private int height;

        public Node[,] girdNode;

        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;
            girdNode = new Node[width, height];
            for(int x=0;x<width;x++)
            {
                for (int y = 0; y < height; y++)
                {
                    girdNode[x,y] = new Node(new Vector2Int(x, y));
                }
            }
            
        }

        public Node GetGridNode(int xPos,int yPos)
        {
            if (xPos < width && yPos < height)
            {
                return girdNode[xPos, yPos];
            }
            Debug.Log("超出网格范围");
            return null;
        }
    }
}
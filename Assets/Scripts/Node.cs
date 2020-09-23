namespace Assets.Scripts
{
    public class Node
    {
        public Node(int vertexId, float foodAmount = 0)
        {
            VertexId = vertexId;
            FoodAmount = foodAmount;
        }

        public Node SetPosition((int posX, int posY) v)
        {
            PosY = v.posY;
            PosX = v.posX;
            return this;
        }
        public float FoodAmount { get; set; }
        public int VertexId { get; }
        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public bool HasFood => FoodAmount > 0;
    }
}
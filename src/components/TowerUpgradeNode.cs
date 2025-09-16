namespace _2d_td;

#nullable enable
public class TowerUpgradeNode
{
    public string Name { get; private set; }
    public int Price;
    public TowerUpgradeNode? Parent { get; private set; }
    public TowerUpgradeNode? LeftChild { get; private set; }
    public TowerUpgradeNode? RightChild { get; private set; }

    public TowerUpgradeNode(string name, int price, TowerUpgradeNode? parent = null,
        TowerUpgradeNode? leftChild = null, TowerUpgradeNode? rightChild = null)
    {
        Name = name;
        Price = price;
        Parent = parent;
        LeftChild = leftChild;
        RightChild = rightChild;

        if (leftChild != null)
        {
            leftChild.Parent = this;
        }

        if (rightChild != null)
        {
            rightChild.Parent = this;
        }
    }
}

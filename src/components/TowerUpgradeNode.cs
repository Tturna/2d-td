namespace _2d_td;

#nullable enable
public class TowerUpgradeNode
{
    public string Name { get; private set; }
    public TowerUpgradeNode? Parent { get; private set; }
    public TowerUpgradeNode? LeftChild { get; private set; }
    public TowerUpgradeNode? RightChild { get; private set; }

    public TowerUpgradeNode(string name, TowerUpgradeNode? parent = null,
        TowerUpgradeNode? leftChild = null, TowerUpgradeNode? rightChild = null)
    {
        Name = name;
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

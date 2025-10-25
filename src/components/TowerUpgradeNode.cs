using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

#nullable enable
public class TowerUpgradeNode
{
    public string Name { get; private set; }
    public Texture2D? UpgradeIcon { get; private set; }
    public int Price { get; private set; }
    public string? Description { get; set; }
    public TowerUpgradeNode? Parent { get; private set; }
    public TowerUpgradeNode? LeftChild { get; private set; }
    public TowerUpgradeNode? RightChild { get; private set; }

    public TowerUpgradeNode(string name, Texture2D? upgradeIcon, int price,
        string? description = null, TowerUpgradeNode? parent = null,
        TowerUpgradeNode? leftChild = null, TowerUpgradeNode? rightChild = null)
    {
        Name = name;
        UpgradeIcon = upgradeIcon;
        Price = price;
        Parent = parent;
        LeftChild = leftChild;
        RightChild = rightChild;
        Description = description;

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

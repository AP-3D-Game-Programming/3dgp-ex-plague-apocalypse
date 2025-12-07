
using System.Collections.Generic;

public enum BulletAction
{
    Destroy,      // Standaard: Kogel gaat kapot
    PassThrough,  // Piercing: Kogel vliegt door (negeer physics botsing)
    Bounce        // Bouncing: Kogel ketst af
}
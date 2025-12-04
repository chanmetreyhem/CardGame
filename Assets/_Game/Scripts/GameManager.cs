using UnityEngine;

[RequireComponent (typeof(AudioManager),typeof(PlayerMove),typeof(Rigidbody))]
public class GameManager : Singleton<GameManager>
{
        public int index = 0;
    public void Start()
    {
          GetComponent<Rigidbody>().isKinematic = true;
        var playerMovement = GetComponent<PlayerMove>();
        playerMovement = new PlayerMove { 
          x = 1,y=4
       };
        print(playerMovement.x);
    }
}

public class AudioManager
{

}
public class PlayerMove
{
    public float x { set; get; }
    public float y { set; get; }
}

interface IModellable
{
     public void MoveLeft();
    public void MoveRight();
}

public class RoadVircle : IModellable
{
    public float x { set; get; }
    public float y { set; get; }
    public virtual void MoveLeft()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MoveRight()
    {
        throw new System.NotImplementedException();
    }
}

public class Car : RoadVircle
{
    public override void MoveLeft()
    {
        base.MoveLeft();
    }

    public override void MoveRight() { 

    }
}

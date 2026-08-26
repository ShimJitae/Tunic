using UnityEngine;

/*
캐릭터는 이동하기 전에 MoveInfo를 먼저 설정해주고, Move()를 실행한다.
*/
public interface IMoveStrategy
{
    public Vector3 MoveInfo { get; set; }
    public void Move();
}

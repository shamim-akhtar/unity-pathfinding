using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float Speed = 1.0f;
    public Queue<Vector2> mWayPoints = new Queue<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Coroutine_MoveTo());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMoveToPoint(Vector2 pt)
    {
        mWayPoints.Enqueue(pt);
    }

    public IEnumerator Coroutine_MoveTo()
    {
        while(true)
        {
            while(mWayPoints.Count > 0)
            {
                yield return StartCoroutine(Coroutine_MoveToPoint(mWayPoints.Dequeue(), Speed));
            }
            yield return null;
        }
    }

    // coroutine to swap tiles smoothly
    private IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }

    IEnumerator Coroutine_MoveToPoint(Vector2 p, float speed)
    {
        Vector3 endP = new Vector3(p.x, p.y, transform.position.z);
        float duration = (transform.position - endP).magnitude / speed;
        yield return StartCoroutine(Coroutine_MoveOverSeconds(transform.gameObject, endP, duration));
    }
}

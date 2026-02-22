using UnityEngine;

[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
public class PlayerSpriteFacing : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private string directionParameter = "Direction";
    [SerializeField] private string isMovingParameter = "IsMoving";
    [SerializeField] private int leftDirectionValue = 3;
    [SerializeField] private int sideDirectionValue = 2;
    [SerializeField] private int upDirectionValue = 1;
    [SerializeField] private int downDirectionValue = 0;
    [SerializeField] private int rightDirectionValue = 2;
    [SerializeField] private bool overrideAnimatorParameters = true;

    private int directionParamHash;
    private int isMovingParamHash;
    private bool shouldFlipLeft;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        directionParamHash = Animator.StringToHash(directionParameter);
        isMovingParamHash = Animator.StringToHash(isMovingParameter);
    }

    private void LateUpdate()
    {
        if (animator == null || targetRenderer == null)
        {
            return;
        }

        if (overrideAnimatorParameters)
        {
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);
            bool up = Input.GetKey(KeyCode.W);
            bool down = Input.GetKey(KeyCode.S);

            bool isMoving = left || right || up || down;
            int direction = animator.GetInteger(directionParamHash);

            // Match existing movement priority: horizontal first, vertical overrides if pressed.
            if (left)
            {
                direction = sideDirectionValue;
                shouldFlipLeft = true;
            }
            else if (right)
            {
                direction = rightDirectionValue;
                shouldFlipLeft = false;
            }

            if (up)
            {
                direction = upDirectionValue;
                shouldFlipLeft = false;
            }
            else if (down)
            {
                direction = downDirectionValue;
                shouldFlipLeft = false;
            }

            animator.SetInteger(directionParamHash, direction);
            animator.SetBool(isMovingParamHash, isMoving);
        }
        else
        {
            shouldFlipLeft = animator.GetInteger(directionParamHash) == leftDirectionValue;
        }

        targetRenderer.flipX = shouldFlipLeft;
    }
}

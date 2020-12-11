using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Electric : Element {
  const float SIZE = 0.5F;

  [SerializeField]
  private bool attacked = false;
  [SerializeField]
  private float attackCooldown = 0.05f;
  [SerializeField]
  private float attackTime = 0;
  [SerializeField]
  private float attackImpulse = 2f;
  [SerializeField]
  private float attackExeTime = 0.32f;

  [SerializeField]
  private bool blocked = false;
  [SerializeField]
  private float blockCooldown = 0.4f;
  [SerializeField]
  private float blockTime = 0;
  [SerializeField]
  private float blockExeTime = 1f;

  [SerializeField]
  private bool dashed = false;
  [SerializeField]
  private bool throwed = false;
  private Vector2 throwPoint = Vector2.zero;
  [SerializeField]
  private float dashCooldown = 0.2f;
  [SerializeField]
  private float dashTime = 0;
  [SerializeField]
  private float dashForce = 0.5f;
  [SerializeField]
  private float dashExeTime = 1f;

  private MovementScript movement;
  private Rigidbody2D rigidbody;

  private float angle = 0;

  private GameObject effect;

  private GroundedController grounded;
  // Start is called before the first frame update
  void Start() {
    movement = GetComponent<MovementScript>();
    rigidbody = GetComponent<Rigidbody2D>();
    grounded = GameObject.Find("Grounded").GetComponent<GroundedController>();
  }

  // Update is called once per frame
  void Update() {
    HandleAttack();
    HandleBlock();
    HandleDash();
  }

  void HandleAttack() {

    if (attacked && attackTime >= attackExeTime) {
      CancelAttack();
    } else if (attacked) {
      ResetVelocity();
      attackTime += Time.deltaTime;

      effect.transform.localScale = new Vector3(SIZE, attackTime * SIZE * 2, 1);
    } else if (!attacked && attackTime > 0) {
      attackTime -= Time.deltaTime;
    } else if (attackTime < 0) {
      attackTime = 0;
    }

  }

  void HandleBlock() {
    if (blocked && blockTime >= blockExeTime) {
      CancelBlock();
    } else if (blocked) {
      blockTime += Time.deltaTime;
      if (!throwed) {
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
      }
    } else if (!blocked && blockTime > 0) {
      blockTime -= Time.deltaTime;
    } else if (blockTime < 0) {
      blockTime = 0;
    }

    if (throwed) {
      if (!effect) {
        effect = Instantiate(abilities.electricBlock, this.transform.position, Quaternion.identity);
      }
      var distance = Vector2.Distance(effect.transform.localScale, throwPoint) / 2f;
      effect.transform.localScale += new Vector3(distance, distance, 0);

      ResetVelocity();
      if (distance >= BlockRange) {
        CancelBlock();
      }
    }
  }


  void HandleDash() {
    if (dashed && dashTime >= dashExeTime) {
      CancelDash();

    } else if (dashed) {
      dashTime += Time.deltaTime;
      var direction = Vector2.ClampMagnitude(throwPoint - (Vector2)this.transform.position, dashForce);
      ResetVelocity();

      rigidbody.MovePosition(this.transform.position + (Vector3)direction);
      float distance = Vector2.Distance((Vector2)rigidbody.transform.position, throwPoint);

      if (distance >= 0 && distance <= 0.03) {
        CancelDash();
      }
    } else if (!dashed && dashTime > 0) {
      dashTime -= Time.deltaTime;
    } else if (dashTime < 0) {
      dashTime = 0;
    }
  }


  void CancelAttack() {
    attacked = false;
    executing = false;
    if (effect) {
      Destroy(effect);
    }
    attackTime = attackCooldown;
  }

  void CancelDash() {
    dashed = false;
    executing = false;
    if (effect) {
      Destroy(effect);
    }

    dashTime = dashCooldown;
  }

  void CancelBlock() {
    blocked = false;
    if (effect) {
      Destroy(effect);
    }
    executing = false;
    throwed = false;
    blockTime = blockCooldown;
  }

  void ResetVelocity() {
    if (executing) {
      rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
    }
  }


  public override void Attack(InputAction.CallbackContext context) {
    if (!executing && attackTime == 0) {
      if (context.started) {
        var teste = context.ReadValue<float>();

        var attackPoint = GameObject.Find("AttackPoint").GetComponent<Transform>();
        Vector3 position = attackPoint.position + (new Vector3(AttackRange / 2f, 0, 0));
        this.gameObject.GetComponent<Animator>().Play(grounded.isGrounded ? "attack" : "jumpAttack");


        angle = Vector2.Angle(movement.Direction, Vector2.right);
        angle = angle > 90 ? angle % 90 : angle;
        directionAttack = movement.Direction.normalized;
        Knockback = knockback;

        attackPoint.parent.rotation = Quaternion.Euler(0, directionAttack.x > 0 ? 0 : 180, directionAttack.y > 0 ? angle : -angle);
        effect = Instantiate(abilities.electricAttack, attackPoint.position, Quaternion.identity, attackPoint);
        effect.transform.localRotation = Quaternion.Euler(0, directionAttack.x > 180 ? 0 : 180, -90);
        effect.transform.localScale = Vector2.zero;

        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(directionAttack.normalized * (-attackImpulse), ForceMode2D.Impulse);

        attacked = true;
        executing = true;
      }
    }
  }

  public override void Block(InputAction.CallbackContext context) {
    if (context.started) {
      if (!executing && blockTime == 0) {

        if (effect) {
          Destroy(effect);
        }
        effect = Instantiate(abilities.electricBlock, this.transform.position, Quaternion.identity, this.transform);
        directionAttack = Vector2.zero;
        Knockback = knockback / 2f;
        blocked = true;
        executing = true;
      } else if (!throwed && blocked && effect) {
        var attackPoint = GameObject.Find("AttackPoint").GetComponent<Transform>();
        throwPoint = new Vector3(BlockRange, BlockRange, 0);
        Knockback = knockback;
        this.gameObject.GetComponent<Animator>().Play(grounded.isGrounded ? "attack" : "jumpAttack");


        blockTime = 0;
        throwed = true;
      }
    }

  }

  public override void Dash(InputAction.CallbackContext context) {
    if (context.canceled) {
      if (dashed) {
        CancelDash();
      }

    }

    if (context.started) {
      if (!executing && dashTime == 0) {
        var rigidbody = GetComponent<Rigidbody2D>();
        throwPoint = rigidbody.position + (new Vector2(DashRange * movement.Direction.x, DashRange * movement.Direction.y));
        directionAttack = movement.Direction.normalized;
        Knockback = knockback / 2f;

        var attackPoint = GameObject.Find("point").GetComponent<Transform>();

        effect = Instantiate(abilities.electricDash, this.transform.position, Quaternion.identity);
        effect.transform.parent = attackPoint;


        angle = Vector2.Angle(movement.Direction, Vector2.right);
        angle = angle > 90 ? angle % 90 : angle;

        attackPoint.rotation = Quaternion.Euler(0, directionAttack.x > 0 ? 180 : 0, directionAttack.y > 0 ? -angle : angle);

        effect.transform.localRotation = Quaternion.Euler(0, directionAttack.x > 180 ? 0 : 180, -90);

        ResetVelocity();
        dashed = true;
        executing = true;
      }
    }

  }


  void OnDrawGizmos() {
    Gizmos.color = Color.red;
    if (blocked) {
      Gizmos.DrawSphere(this.transform.position, BlockRange);
    }
  }

  void OnCollisionEnter2D(Collision2D other) {
    if (!grounded.isGrounded && dashed) {
      CancelDash();
    }
  }
}

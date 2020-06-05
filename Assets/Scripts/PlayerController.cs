using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float longIdleTime = 5f;  //Variable para determinar cuánto tiempo debe pasar para que inicie el long idle
	public float speed = 2.5f;  //Velocidad que se le aplicará al personaje para moverse
	public float jumpForce = 2.5f;  //Velocidad que se le aplicará al personaje para saltar

	public Transform groundCheck;  //Vamos a tomar del transform el floorpoint del personaje
	public LayerMask groundLayer;  //No entendí muy bien
	public float groundCheckRadius;  //No entendí muy bien

	// References
	private Rigidbody2D _rigidbody;  //Obtenemos el componente RigidBody del objeto que tenga este script 
	private Animator _animator;  //Obtenemos el componente Animator del objeto que tenga este script 

	// Long Idle
	private float _longIdleTimer;  //Contador para saber cuanto tiempo llevamos dentro de Ilde leyendo la etiqueta "Idle"

	// Movement
	private Vector2 _movement;  //Declaramos el vector de movimiento 2D (Izquierda(-0) - Derecha(+0))
	private bool _facingRight = true;  //Variable que indica hacia donde está mirando el player, por defecto es la derecha
	private bool _isGrounded;  //Boolenao que nos dirá si sí estámos en el suelo, o no

	// Attack
	private bool _isAttacking;  //Booleano que me indica si estoy atacando o no


	void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();  //Obtenemos el componente RigidBody del objeto que tenga este script
		_animator = GetComponent<Animator>();  //Obtenemos el componente Animator del objeto que tenga este script
	}

	void Start()
    {
        
    }

    void Update()
    {
		if (_isAttacking == false) {  //Solo lee esto si no estoy atacando
			// Movement
			float horizontalInput = Input.GetAxisRaw("Horizontal");  //Recibimos la entrada por teclado (Raw es más para 2-D)
			_movement = new Vector2(horizontalInput, 0f);  //La variable _movement ahora tiene el valor de entrada y lo aplica al eje x , el 0f significa que no importa el eje y

			// Flip character  Condición que verifica hacia dónde está mirando el personaje
			if (horizontalInput < 0f && _facingRight == true) {  //Si la entrada de teclado es izquierda (-1) y el personaje está mirando a la derecha, llama al método Flip()
				Flip();
			} else if (horizontalInput > 0f && _facingRight == false) {  //Si la entrada de teclado es derecha (1) y el personaje no está mirando a la derecha, llama al método Flip()
				Flip();
			}
		}

		// Is Grounded?
		_isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);  //Va a checar las interacciones que está teniendo con las demás capas, y asignará true o false dependiendo del resultado

		// Is Jumping?
		if (Input.GetButtonDown("Jump") && _isGrounded == true && _isAttacking == false) {  //Si la entrada por teclado es igual al axis de salto, y estás en el piso, y no estás atacando, entra a la condición
			_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);  //Al rigidbody le vamos a decir mediante la clase AddForce, que a nuentro vector 2 le agregue una fuerza hacia arriba, multiplicando 1(?) por la fuerza de salto que hemos asignado arriba, con esa fuerza, añadimos un impulso explosivo con la clase ForceMode2D
		}

		// Wanna Attack?
		if (Input.GetButtonDown("Fire1") && _isGrounded == true && _isAttacking == false) {  //Si la entrada de teclado es de atacar y estoy en el piso pero también no estoy atacando, entra a la condición
			_movement = Vector2.zero;  //Se detiend en seco el vector de movimiento
			_rigidbody.velocity = Vector2.zero;  //Disminuyo la velocidad del rigid body a 0 para que no se mueva mientras ataca
			_animator.SetTrigger("Attack");  //Ejecuto el trigger de ataque para que entre en esa animación
		}
	}

	void FixedUpdate()
	{
		if (_isAttacking == false) {  //Si no estoy atacando, me muevo
			float horizontalVelocity = _movement.normalized.x * speed;  //Tomamos el eje x (-0 o +0) en el que se está moviendo el personaje y lo multiplicamos por la velocidad que asignamos arriba
			_rigidbody.velocity = new Vector2(horizontalVelocity, _rigidbody.velocity.y);  //Al componente velocity del RigidBody le aplicamos la fuerza que obtuvimos arriba y la fuerza que ya posee
			/*Es importante agregar la velocidad de "y" para que la gravedad pueda afectar al componente*/
		}
	}

	void LateUpdate()
	{
		_animator.SetBool("Idle", _movement == Vector2.zero);  //Modificamos el booleano de el animator del player, especificamos el nombre del booleano y sólo será verdad cuando lo de después de la coma sea true
		_animator.SetBool("IsGrounded", _isGrounded);  //Modificamos el valor del booleano "IsGrounded" para que afecte a las animaciones
		_animator.SetFloat("VerticalVelocity", _rigidbody.velocity.y);  //A la variable "VerticalVelocity" le asignamos el valor ya sea de subida o de bajada que tiene nuestro personaje, en el animator está especificado con qué valores se mostrará qué animación, si de acenso o descenso

		// Animator
		if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {  //Obtengo el estado actual del animator, si su etiqueta coincide con "Attack" actualizo el booleano a true para saber que estoy actualmente atacando
			_isAttacking = true;
		} else {  //Si la etiqueta no coincide con "Attack" actualizo el valor del booleano a false para saber que no estoy atacando
			_isAttacking = false;
		}

		// Long Idle
		if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle")) {  //Obtengo el estado actual del animator, si su etiqueta coincide con "Idle" entro en la condición
			_longIdleTimer += Time.deltaTime;  //En cada actualización de frames, le voy sumando 1 a la variable que sirve de contador

			if (_longIdleTimer >= longIdleTime) {  //Si el contador es igual a la variable de tiempo límite que asignamos arriba, ejecuto el trigger "LongIdle"
				_animator.SetTrigger("LongIdle");
			}
		} else {  //Si no estoy en la etiqueta "Idle" reseteo mi contador a 0 parq eu no se cumule
			_longIdleTimer = 0f;
		}
	}

	private void Flip()
	{
		_facingRight = !_facingRight;  //Una vez que el método sea llamado, cambiará el valor del booleano al contrario del que tiene en ese momento
		float localScaleX = transform.localScale.x;  //Tomamos del transform del personaje el componente Scale y de ese mismo, el componente x
		localScaleX = localScaleX * -1f;  //Multiplicamos el valor que tenga el componente x por -1 para invertir la orientación del sprite, (-1 * -1 = 1) (1 * -1 = -1) 
		transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);  //Asignamos al Scale del componnte actual, el valor que obtuvimos arriva, y los dos restantes (y,z) los dejamos por dejecto los que tenga, sin embargo si es importante colocarlos
	}
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
  private GameObject player;
  public float cameraSpeed = 4f;


  // Start is called before the first frame update
  void Start() {
    player = GameObject.Find("Player");
  }

  // Update is called once per frame
  void Update() {
    if (player) {
      Vector3 targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, this.transform.position.z);
      this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, cameraSpeed * Time.deltaTime); 
    }
  }
}

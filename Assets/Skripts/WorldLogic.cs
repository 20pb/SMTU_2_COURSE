﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;
using System;

public class WorldLogic : MonoBehaviour
{
    public GameObject submarine, mine, chainLink, fixOnTheGround, lighthouse; // 3d модели

    // Материалы для 3d моделей
    public Material generalMaterial, invisibleMaterial, sandMaterial;
    public Mesh generalMesh;
    public static Material materialForMiniCube;

    private GameObject model3D; // используемая в данный момент 3d модель
    private byte modelID; // индификационный номер(далее ID) используемой 3d модели

    private static int step = MapSizeEditor.step; // step - размер одной клетки. Пр. 1 кл = 10 у.е

    public Camera camera; // камера на сцене, через которую смотрит игрок. Нужна для использования Raycast

    private GameObject newMiniCube; // временный объект, созданный игроком при нажатии на экран

    GameObject[,,] TypeOfObjectOnMap; // массив с объектами для удобного удаления их с сцены
    public static byte[,,] TypeOfObjectOnMapInt; // массив с ID объектов

    private byte MainX,MainY,MainZ; // Координаты главной лодки
    private byte EndX,EndY,EndZ; // Координаты конечного пункта пути

    private float mouseScrollValue; // для редактирования высоты с помощью колёсика мышки

    // Временные переменные, чтобы не создавать их каждый frame
    private float positionX, positionY, positionZ;
    private float differenceX, differenceY, differenceZ;
    int xCoord, yCoord, zCoord;
    int tempxCoord = 0, tempzCoord = 0;

    bool Boolfas, deleteMode;
    WorldLogic()
    {
        // инициализируем массивы с размерами, указанными пользователем
        TypeOfObjectOnMap = new GameObject[MapSizeEditor.countX, MapSizeEditor.countY, MapSizeEditor.countZ];
        TypeOfObjectOnMapInt = new byte[MapSizeEditor.countX, MapSizeEditor.countY, MapSizeEditor.countZ];
    }
    public void IsTapped(int num) // вызывается при нажатии на панель (рис.1)
    {
        if (newMiniCube == null)
        {
            modelID = (byte)num;
            switch (num)
            {
                case 1:
                    model3D = submarine;
                    deleteMode = false;
                    break;
                case 2:
                    model3D = mine;
                    deleteMode = false;
                    break;
                case 3:
                    model3D = lighthouse;
                    deleteMode = false;
                    break;
                case 6:
                    model3D = null;
                    deleteMode = false;
                    break;
                case 7:
                    deleteMode = true;
                    break;
                case 9:
                    model3D = null;
                    deleteMode = false;
                    break;
                default:
                    deleteMode = false;
                    model3D = null;
                    break;
            }
        }
    }

    private void Start()
    {
        gameObject.transform.localScale = new Vector3(MapSizeEditor.countX * step, MapSizeEditor.countY * step, MapSizeEditor.countZ * step);
        
        for(int x = 0; x < MapSizeEditor.countX; x++)
        {
            for (int y = 0; y<1; y++)
            {
                for (int z = 0; z < MapSizeEditor.countZ; z++)
                {
                    xCoord = (int)(step * (x));
                    yCoord = (int)(step * (y));
                    zCoord = (int)(step * (z));
                    newMiniCube = new GameObject();
                    newMiniCube.transform.position = new Vector3(xCoord, yCoord, zCoord);
                    newMiniCube.transform.localScale = new Vector3(step, step, step);
                    newMiniCube.transform.parent = transform;
                    newMiniCube.AddComponent<MeshCollider>().sharedMesh = generalMesh;

                    newMiniCube.AddComponent<MeshRenderer>().material = generalMaterial;
                    newMiniCube.AddComponent<MeshFilter>().mesh = generalMesh;

                    TypeOfObjectOnMap[x, y, z] = newMiniCube;
                    TypeOfObjectOnMapInt[x, y, z] = 9;
                    newMiniCube.tag = "ground";
                }
            }
        }
        newMiniCube = null;
        xCoord = 0;
        yCoord = 0; 
        zCoord = 0;
        materialForMiniCube = sandMaterial;
    }

    /// <summary> Метод, отвечающий за инициализацию 
    /// <see cref="UnityEngine.GameObject"/> 
    ///  на сцене
    ///  <para> Является методом класса <seealso cref="WorldLogic"/></para>
    /// </summary>
    void SpawnerControl(GameObject model3D)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (newMiniCube == null)
            {
                if (model3D != null || modelID == 9)
                {
                    newMiniCube = new GameObject();
                    newMiniCube.transform.localScale = new Vector3(step + 2, step + 2 , step + 2);
                    newMiniCube.AddComponent<MeshFilter>().mesh = generalMesh;
                    newMiniCube.AddComponent<ObjectID>();

                    if (modelID == 9) newMiniCube.AddComponent<MeshRenderer>().material = generalMaterial;
                    else Instantiate(model3D, newMiniCube.transform);
                
                    if (model3D == mine) newMiniCube.tag = "mine";
                }
            }
            else
            {
                if (xCoord >= 0 && xCoord < MapSizeEditor.countX)
                {
                    if (yCoord >= 0 && yCoord < MapSizeEditor.countY)
                    {
                        if (zCoord >= 0 && zCoord < MapSizeEditor.countZ)
                        {
                            if (TypeOfObjectOnMap[xCoord, yCoord, zCoord] == null)
                            {
                                newMiniCube.transform.localScale = new Vector3(step , step , step );
                                newMiniCube.transform.parent = transform;
                                newMiniCube.AddComponent<BoxCollider>().center = new Vector3(0, 0, 0);
                                TypeOfObjectOnMap[xCoord, yCoord, zCoord] = newMiniCube;
                                TypeOfObjectOnMapInt[xCoord, yCoord, zCoord] = modelID;

                                if (modelID == 1)
                                {
                                    if (!(MainX == 0 && MainY == 0 && MainZ == 0))
                                    {
                                        Destroy(TypeOfObjectOnMap[MainX, MainY, MainZ]);
                                        TypeOfObjectOnMapInt[MainX, MainY, MainZ] = 0;
                                    }
                                    MainX = (byte)xCoord; MainY = (byte)yCoord; MainZ = (byte)zCoord;
                                }
                                else if (modelID == 3)
                                {
                                    if (!(EndX == 0 && EndY == 0 && EndZ == 0))
                                    {
                                        Destroy(TypeOfObjectOnMap[EndX, EndY, EndZ]);
                                        TypeOfObjectOnMapInt[EndX, EndY, EndZ] = 0;
                                    }
                                    EndX = (byte)xCoord; EndY = (byte)yCoord; EndZ = (byte)zCoord;
                                }
                                if (model3D == mine)
                                {
                                    for (int i = yCoord; i>= 1; i--)
                                    {
                                        if (TypeOfObjectOnMap[xCoord, i - 1, zCoord] != null)
                                        {
                                            if (TypeOfObjectOnMap[xCoord, i - 1, zCoord].tag == "mine")
                                            {
                                                Destroy(TypeOfObjectOnMap[xCoord, i - 1, zCoord]);
                                                TypeOfObjectOnMap[xCoord, i - 1, zCoord] = null;
                                                TypeOfObjectOnMapInt[xCoord, i - 1, zCoord] = 0;
                                            }
                                        }
                                        if (TypeOfObjectOnMap[xCoord, i - 1, zCoord] != null)
                                        {
                                            if (TypeOfObjectOnMap[xCoord, i - 1, zCoord].tag != "chain")
                                            {
                                                if (TypeOfObjectOnMap[xCoord, i, zCoord].tag != "mine")
                                                {
                                                    newMiniCube = new GameObject();
                                                    newMiniCube.transform.localScale = new Vector3(step, step, step);
                                                    newMiniCube.AddComponent<MeshFilter>().mesh = generalMesh;
                                                    newMiniCube.transform.position = new Vector3(xCoord * step, i * step, zCoord * step);
                                                    newMiniCube.transform.parent = transform;
                                                    newMiniCube.AddComponent<BoxCollider>().center = new Vector3(0, 0, 0);
                                                    Destroy(TypeOfObjectOnMap[xCoord, i, zCoord]);
                                                    Instantiate(fixOnTheGround, newMiniCube.transform);
                                                    Instantiate(chainLink, newMiniCube.transform);
                                                    newMiniCube.tag = "chain";
                                                    TypeOfObjectOnMap[xCoord, i, zCoord] = newMiniCube;
                                                    newMiniCube = null;
                                                    break;
                                                }
                                                else
                                                {
                                                    Instantiate(fixOnTheGround, newMiniCube.transform);
                                                    TypeOfObjectOnMap[xCoord, i, zCoord] = newMiniCube;
                                                    newMiniCube = null;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        yCoord = i - 1;
                                        xCoord = tempxCoord;
                                        zCoord = tempzCoord;
                                        newMiniCube = new GameObject();
                                        newMiniCube.transform.localScale = new Vector3(step, step, step);
                                        newMiniCube.AddComponent<MeshFilter>().mesh = generalMesh;
                                        newMiniCube.transform.position = new Vector3(xCoord * step, yCoord * step, zCoord * step);
                                        newMiniCube.transform.parent = transform;
                                        newMiniCube.AddComponent<BoxCollider>().center = new Vector3(0, 0, 0);
                                        Instantiate(chainLink, newMiniCube.transform);
                                        newMiniCube.tag = "chain";
                                        TypeOfObjectOnMap[xCoord, yCoord, zCoord] = newMiniCube;
                                        TypeOfObjectOnMapInt[xCoord, yCoord, zCoord] = 8;
                                    }
                                }
                                newMiniCube = null;
                                mouseScrollValue = 0;
                            }
                        }
                    }
                }
            }
        }
        if (newMiniCube != null)
        {
            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0) mouseScrollValue += Input.mouseScrollDelta.y; // смена высоты колёсиком мышки
            
            Ray ray = camera.ScreenPointToRay(Input.mousePosition); // используем Raycast для определения местоположения мышки на 3d пространстве
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000 * step))
            {
                // получаем координаты, куда попал луч и переводим их в удобный для нас вид
                differenceX = Mathf.Abs(hit.point.x - hit.collider.gameObject.transform.position.x) < step / 2 - 0.0001f ? 0 : hit.point.x - hit.collider.gameObject.transform.position.x;
                differenceY = Mathf.Abs(hit.point.y - hit.collider.gameObject.transform.position.y) < step / 2 - 0.0001f ? 0 : hit.point.y - hit.collider.gameObject.transform.position.y;
                differenceZ = Mathf.Abs(hit.point.z - hit.collider.gameObject.transform.position.z) < step / 2 - 0.0001f ? 0 : hit.point.z - hit.collider.gameObject.transform.position.z;
                
                // сопоставляем эти координаты с координатами объекта, в который попал луч
                positionX = hit.collider.gameObject.transform.position.x + differenceX * 2;
                positionY = hit.collider.gameObject.transform.position.y + differenceY * 2 + mouseScrollValue * step;
                positionZ = hit.collider.gameObject.transform.position.z + differenceZ * 2;

                if (positionY >= 0 && positionY / step + 0.1 < MapSizeEditor.countY)
                {
                    yCoord = (int)(positionY / step + 0.1);
                    if (positionX >= 0 && positionX / step + 0.1 < MapSizeEditor.countX)
                    {
                        tempxCoord = (int)(positionX / step + 0.1);
                    }
                    if (positionZ >= 0 && positionZ/ step + 0.1 < MapSizeEditor.countZ)
                    {
                        tempzCoord = (int)(positionZ / step + 0.1);
                    }
                }
                else
                {
                    mouseScrollValue = 0;
                }
                Boolfas = false;
                for (int i = yCoord; i < MapSizeEditor.countY; i++)
                {
                    if (TypeOfObjectOnMap[tempxCoord, i, tempzCoord] == null)
                    {
                        xCoord = tempxCoord;
                        yCoord = i;
                        zCoord = tempzCoord;
                        Boolfas = true;
                        break;
                    }
                }
                if(!Boolfas)
                { 
                    for (int i = MapSizeEditor.countY-1; i >= 0; i--)
                    {
                        if (TypeOfObjectOnMap[tempxCoord, i, tempzCoord] == null)
                        {
                            xCoord = tempxCoord;
                            yCoord = i;
                            zCoord = tempzCoord;
                            break;
                        }
                    }
                }
                if (TypeOfObjectOnMap[xCoord, yCoord, zCoord] == null) newMiniCube.transform.position = new Vector3(xCoord * step, yCoord * step, zCoord * step);
            }
        }
    }
    /// <summary>
    /// Метод, который удаляет <see cref="UnityEngine.GameObject"/> со сцены 
    /// </summary>
    /// <remarks><see cref="UnityEngine.GameObject"/> выбирается с помощью <see cref="UnityEngine.Physics.Raycast(Ray, out RaycastHit, float)"/>
    /// </remarks>
    void DeleteControl()
    {
        if (Input.GetMouseButtonDown(0) && deleteMode)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 100 * step))
            {
                if (hit.collider.gameObject.tag != "ground")
                {
                    if (hit.collider.gameObject.tag != "chain")
                    {
                        Destroy(hit.collider.gameObject);
                        TypeOfObjectOnMapInt[(int)(hit.collider.gameObject.transform.position.x / step + 0.1), (int)(hit.collider.gameObject.transform.position.y / step + 0.1), (int)(hit.collider.gameObject.transform.position.z / step + 0.1)] = 0;
                        Debug.Log("M[" + (int)(hit.collider.gameObject.transform.position.x / step + 0.1) + ":" + (int)(hit.collider.gameObject.transform.position.y / step + 0.1) + ":" + (int)(hit.collider.gameObject.transform.position.z / step + 0.1) + "]" + TypeOfObjectOnMapInt[(int)(hit.collider.gameObject.transform.position.x / step + 0.1), (int)(hit.collider.gameObject.transform.position.y / step + 0.1), (int)(hit.collider.gameObject.transform.position.z / step + 0.1)]);
                    }
                    if (hit.collider.gameObject.tag == "mine")
                    {
                        for (int i = (int)(hit.collider.gameObject.transform.position.y / step + 0.1); i >= 0; i--)
                        {
                            if (TypeOfObjectOnMap[(int)(hit.collider.gameObject.transform.position.x / step + 0.1), i, (int)(hit.collider.gameObject.transform.position.z / step + 0.1)].tag == "chain")
                            {
                                Destroy(TypeOfObjectOnMap[(int)(hit.collider.gameObject.transform.position.x / step + 0.1), i, (int)(hit.collider.gameObject.transform.position.z / step + 0.1)]);
                                TypeOfObjectOnMapInt[(int)(hit.collider.gameObject.transform.position.x / step + 0.1), i, (int)(hit.collider.gameObject.transform.position.z / step + 0.1)] = 0;
                                Debug.Log("M[" + (int)(hit.collider.gameObject.transform.position.x / step + 0.1) + ":" + i + ":" + (int)(hit.collider.gameObject.transform.position.z / step + 0.1) + "]" + TypeOfObjectOnMapInt[(int)(hit.collider.gameObject.transform.position.x / step + 0.1), i, (int)(hit.collider.gameObject.transform.position.z / step + 0.1)]);
                            }
                        }

                    }
                }
            }
        }
    }
    void objectRot(GameObject model)
    {
        if (Input.GetButtonDown("T"))
        {
            model.transform.Rotate(0,90,0);
        }
        if (Input.GetButtonDown("R"))
        {
            model.transform.Rotate(0, -90, 0);
        }
    }
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && !deleteMode)
        {
            SpawnerControl(model3D);
        }
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            DeleteControl();
        }
        if(model3D != null && newMiniCube != null && model3D != mine)
        {
            objectRot(newMiniCube);

        }

    }
}
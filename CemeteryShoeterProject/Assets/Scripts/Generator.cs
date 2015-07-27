using UnityEngine;

using System.Collections;

public class Generator : MonoBehaviour {

    private const float WALL_Y = 2.361404f;
	
    private Terrain lastShapedTerrain;
    public GameObject[] gravePrefabs;
    public float perlinNoiseDensity;
    public float perlinNoiseDivider;

    public Transform rightWall;
    public Transform leftWall;
    public GameObject wallPrefab;
    public GameObject noReturnWall;

    public GameObject gameOverSwitchPrefab;

    public Player player;

    public Terrain entryTerrain;
    public Terrain terrain1;
    public Terrain terrain2;
    public Terrain currentTerrain;
    private Terrain lastTerrainThatAppendedTo;

    public int requiredKills;
    public int killCount;

    public GameObject enemyPrefab;
	
    
    private float nextOffset = 0.0f;
	
    public void Start(){
	   currentTerrain = terrain1;
       
       GenerateTerrainBaseShape(entryTerrain,nextOffset,0,0,0);
       GenerateTerrainBaseShape(terrain1,nextOffset,50,80,40);
       GenerateTerrainBaseShape(terrain2, nextOffset, 50, 80, 40);
       AddGraves(terrain1);
       AddGraves(terrain2);
       AddWalls(340);
       
	}

    void Update(){
        if(Input.GetKeyDown(KeyCode.F)){
            AppendTerrain();
        }
    }

    public void AppendTerrain(){
        lastTerrainThatAppendedTo = currentTerrain;
        Terrain terrainToAppend = getOtherTerrain();
        Vector3 pos = terrainToAppend.GetPosition();
        pos.z = currentTerrain.GetPosition().z + 512.0f;
        terrainToAppend.transform.position = pos;
        GenerateTerrainBaseShape(terrainToAppend, nextOffset, 50, 80, 40);
        AddGraves(terrainToAppend);
        rightWall.transform.position += Vector3.forward * 512.0f;
        leftWall.transform.position += Vector3.forward * 512.0f;
        Vector3 noReturnPos = noReturnWall.transform.position;
        noReturnPos.z = currentTerrain.GetPosition().z;
        noReturnWall.transform.position = noReturnPos;
        
    }



    public void AddWalls(int numberOfWalls){
        int currentNumberOfWalls = rightWall.childCount;
        float currentWallZ = currentNumberOfWalls * 3.0f;

        Vector3 spawnPos = new Vector3(0.0f,0.0f,currentWallZ);
        Vector3 eulerRot = new Vector3(0.0f,270.0f,0.0f);


        for(int i = 0; i < numberOfWalls; i++){
           
           GameObject left = GameObject.Instantiate(wallPrefab) as GameObject;
           GameObject right = GameObject.Instantiate(wallPrefab) as GameObject;
           left.transform.rotation = Quaternion.Euler(eulerRot);
           right.transform.rotation = Quaternion.Euler(eulerRot);
           
           left.transform.parent = leftWall;
           right.transform.parent = rightWall;

           left.transform.localPosition = spawnPos;
           right.transform.localPosition = spawnPos;
           currentWallZ += 3.0f;
           spawnPos.z = currentWallZ;
        }

    }

    public IEnumerator GenerateTerrainBaseShapeCoroutine(Terrain terrain, float xOffset, int numberOfHills, int maxRadius, int minRadius){
        print("Generuje");
        TerrainData data = terrain.terrainData;
        float[,] heightMap = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        float xVal = 0.0f;
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++) {
            for (int j = 0; j < terrain.terrainData.heightmapHeight; j++) {
                xVal = ((float)i / (float)data.heightmapWidth) * perlinNoiseDensity;
                float yVal = ((float)j / (float)data.heightmapHeight) * perlinNoiseDensity;
                heightMap[i, j] = Mathf.PerlinNoise(xVal + xOffset, yVal) / perlinNoiseDivider;
                
            }
            
        }
        nextOffset += xVal;
        

        for (int i = 0; i < numberOfHills; i++) {
            int radius = Random.Range(minRadius, maxRadius);
            int randX = Random.Range(radius, data.heightmapWidth - radius);
            int randY = Random.Range(radius, data.heightmapHeight - radius);
            heightMap = stampHillOnHeighmap(randX, randY, radius, heightMap);
        }


        terrain.terrainData.SetHeights(0, 0, heightMap);

        if (lastShapedTerrain) {
            lastShapedTerrain.SetNeighbors(null, terrain, null, null);
            terrain.SetNeighbors(null, null, null, lastShapedTerrain);
        }

        lastShapedTerrain = terrain;
        print("Wygenerowane");
        yield return null;
    }

    public void GenerateTerrainBaseShape(Terrain terrain, float xOffset, int numberOfHills, int maxRadius, int minRadius){
        StartCoroutine(GenerateTerrainBaseShapeCoroutine(terrain, xOffset, numberOfHills, maxRadius, minRadius));
        
    }
    
    public void AddGraves(Terrain terrain){
        StartCoroutine(AddGravesCoroutine(terrain));
        
    }

    private IEnumerator AddGravesCoroutine(Terrain terrain){
       
        for (int i = terrain.transform.childCount - 1; i >= 0; i--) {
            Destroy(terrain.transform.GetChild(i).gameObject);
       }

        for (int i = 0; i < terrain.terrainData.size.x; i++) {
            for (int j = 0; j < terrain.terrainData.size.z; j++) {
                if (i % 5 == 0 && j % 5 == 0 && i != 0 && j != 0) {
                    if (Random.Range(0.0f, 1.0f) < 0.1f) {
                        AddGrave(i, j, terrain);
                        
                    }
                }
            }
           
        }

        yield return null;
    }

    public void RandomiseTerrainTextures(Terrain terrain){
        TerrainData data = terrain.terrainData;
    }

    public void AddGrave(int x, int y, Terrain terrain){
        TerrainData data = terrain.terrainData;
        float xNormalized = x/(float)data.size.x;
        float yNormalized = x/(float)data.size.y;
        float scaleRatio = Random.Range(0.8f,1.2f);
        
        
        Vector3 spawnPos = Vector3.zero;
        spawnPos.x = x + terrain.GetPosition().x;
        spawnPos.z = y + terrain.GetPosition().z;
        spawnPos.y = terrain.SampleHeight(spawnPos)-0.1f;
        
        GameObject go = GameObject.Instantiate(gravePrefabs[Random.Range(0,gravePrefabs.Length)],spawnPos,Quaternion.identity) as GameObject;
        go.transform.localScale = Vector3.one * scaleRatio;
        go.transform.parent = terrain.transform;
        RaycastHit hitInfo;
        if(Physics.Linecast(go.transform.position + Vector3.up,go.transform.position - Vector3.up * 50.0f,out hitInfo)){
            go.transform.up = hitInfo.normal;
        }
        
        go.transform.RotateAround(go.transform.up,Random.Range(0.0f,360.0f));

    }

    private float[,] stampHillOnHeighmap(int centerX, int centerY, int radius, float[,] map){
        float hillHeight = Random.Range(0.2f,0.6f);

        for (int i = 0; i < map.GetLength(0); i++){
            for (int j = 0; j < map.GetLength(1); j++){
                if (IsInsideCircle(i, j, centerX, centerY,radius)){
                    map[i, j] += hillHeight * Mathfx.Hermite(0.0f,0.70f,1.0f - Distance(i,j,centerX,centerY)/radius);
                }
            }
        }
        return map;
	}

    

    private bool IsInsideCircle(int xPos, int yPos, int centerX, int centerY, int radius){
        return (xPos - centerX) * (xPos - centerX) + (yPos - centerY) * (yPos - centerY) < radius * radius;
    }

    private bool IsCirclePossible(int terrainWidth, int terrainHeight, int centerX, int centerY, int radius){
        return (centerX + radius < terrainWidth && centerX > radius) &&
                (centerY + radius < terrainHeight && centerY > radius);
    }

    private float Distance(int x1, int y1, int x2, int y2){
        return Mathf.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));
    }

    private Terrain getOtherTerrain(){
        if(currentTerrain == terrain1){
            return terrain2;
        }
        return terrain1;
    }

    public void ChangeCurrentTerrain(GameObject go){
        Terrain newTerrain = go.GetComponent<Terrain>();
        
        if(newTerrain == entryTerrain){
            currentTerrain = terrain1;
        }
        else{
            
            if(currentTerrain != newTerrain){
                currentTerrain = newTerrain;
            }

            


        }

        if(player.transform.position.z > currentTerrain.GetPosition().z + 300.0f){
           
           if(lastTerrainThatAppendedTo == null && currentTerrain == terrain1){
                
           }
           else{
               if (lastTerrainThatAppendedTo != currentTerrain) {
                   AppendTerrain();
               }
           }
           
        }
    }

    public void EnemyDied(){
        killCount++;


        if(GameOverSwitch.instance){
            return;
        }

        UserInterface.SetText2(killCount+"/"+requiredKills);

        if(requiredKills <= killCount){
            Terrain other = getOtherTerrain();
            

            SpawnGameOverSwitch(currentTerrain);

            
            
            
            
        }
    }

    public void SpawnGameOverSwitch(Terrain terrain){
        Vector3 spawnPos = terrain.GetPosition() + new Vector3(128.0f, 0.0f, 256.0f);
        GameObject go = GameObject.Instantiate(gameOverSwitchPrefab,spawnPos,Quaternion.Euler(new Vector3(0.0f,0.0f,180.0f))) as GameObject;
        UserInterface.SetText2("Find the light, and be free...");
        Vector3 pos = go.transform.position;
        pos.y = terrain.SampleHeight(go.transform.position) + 0.8f;
        go.transform.position = pos;

    }

    public void StartSpawning(){
        StartCoroutine(SpawnSkeletons());
    }

    private IEnumerator SpawnSkeletons(){
        Vector3 spawnPos;


        while(true){
            if(GameOverSwitch.instance && GameOverSwitch.instance.gameOver){
                break;
            }
            
            
            spawnPos = player.transform.position + Random.onUnitSphere * 50.0f;
            if(Enemy.instances.Count < 10){
                GameObject.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(Random.RandomRange(1.0f,10.0f));
        }


    }

}

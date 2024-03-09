#include <fbxsdk.h>
#define RAPIDJSON_NOMEMBERITERATORCLASS
#include "rapidjson/document.h" 
#include <fstream>
#include <iostream>
#include <filesystem>
#include <sstream>
#include <vector>
#include <windows.h>
#include <shlobj.h>
#include "DataHelpers.h"
#include "MemoryStream.h"
#include "TrackBuilder.h"

namespace fs = std::filesystem;


// Parse json file and load into ExportInformation type
void ParseJson(const rapidjson::Document& doc, ExportInformation& exportInfo, std::vector<PointData>& points, Line& startLine, Line& finishLine) 
{
    try
    {
        const rapidjson::Value& exportInfoJson = doc["export_information"];
        exportInfo.measured_distance = exportInfoJson["measured_distance"].GetDouble();
        exportInfo.measured_scale = exportInfoJson["measured_scale"].GetDouble();
        exportInfo.is_imperial_units = exportInfoJson["is_imperial_units"].GetBool();
        exportInfo.fbx_ratio = exportInfoJson["fbx_ratio"].GetDouble();
        exportInfo.image_file_path = exportInfoJson["image_file_path"].GetString();

        const rapidjson::Value& pointsJson = doc["point_data"];
        for (rapidjson::SizeType i = 0; i < pointsJson.Size(); i++) 
        {
            PointData point;
            point.fbx_x = pointsJson[i]["fbx_x"].GetDouble();
            point.fbx_y = pointsJson[i]["fbx_y"].GetDouble();
            point.rotation = pointsJson[i]["rotation"].GetDouble();
            point.cone_id = pointsJson[i]["cone_id"].GetInt();
            points.push_back(point);
        }

        const rapidjson::Value& startLineJson = doc["start_line"];
        startLine.fbx_x1 = startLineJson["fbx_x1"].GetDouble();
        startLine.fbx_y1 = startLineJson["fbx_y1"].GetDouble();
        startLine.fbx_x2 = startLineJson["fbx_x2"].GetDouble();
        startLine.fbx_y2 = startLineJson["fbx_y2"].GetDouble();
        startLine.angle = startLineJson["angle"].GetDouble();
        startLine.cone_id = startLineJson["cone_id"].GetInt();

        const rapidjson::Value& finishLineJson = doc["finish_line"];
        finishLine.fbx_x1 = finishLineJson["fbx_x1"].GetDouble();
        finishLine.fbx_y1 = finishLineJson["fbx_y1"].GetDouble();
        finishLine.fbx_x2 = finishLineJson["fbx_x2"].GetDouble();
        finishLine.fbx_y2 = finishLineJson["fbx_y2"].GetDouble();
        finishLine.angle = finishLineJson["angle"].GetDouble();
        finishLine.cone_id = finishLineJson["cone_id"].GetInt();
        return;
    }
    catch (std::exception e)
    {
        std::cerr << "Error getting JSON keys" << std::endl;
        throw e;
    }
}


// Load a scene from path
void LoadScene(FbxManager* pSdkManager, FbxScene* pScene, std::string filePath) 
{
    FbxIOSettings* ios = FbxIOSettings::Create(pSdkManager, IOSROOT);
    pSdkManager->SetIOSettings(ios);

    // Create importer using sdk manager.
    FbxImporter* lImporter = FbxImporter::Create(pSdkManager, "");

    // Use first argument as the filename for the importer.
    if (!lImporter->Initialize(filePath.c_str(), -1, pSdkManager->GetIOSettings())) 
    {
        std::stringstream ss;
        ss << "Call to FbxImporter::Initialize() failed.\n" 
            << "Error returned: \n\n" 
            << lImporter->GetStatus().GetErrorString() 
            << std::endl;
        lImporter->Destroy();
        std::exception e(ss.str().c_str());
        throw e;
    }

    lImporter->Import(pScene);
    lImporter->Destroy();
    return;
}


// Build track to an FBX stream
MemoryStream* BuildTrack(const char* assetPath, ExportInformation exportInfo, std::vector<PointData> points, Line startLine, Line finishLine)
{
    // Create SDK manager.
    FbxManager* lSdkManager = FbxManager::Create();
    FbxScene* lFinalScene = FbxScene::Create(lSdkManager, "Final Scene");
    fs::path assetPathP = assetPath;
    assetPathP /= "Assets";

    FbxScene* lGround = FbxScene::Create(lSdkManager, "Ground");
    LoadScene(lSdkManager, lGround, (assetPathP / "track.fbx").string());

    FbxScene* lWall = FbxScene::Create(lSdkManager, "Wall");
    LoadScene(lSdkManager, lWall, (assetPathP / "wall.fbx").string());

    FbxScene* lConeUp = FbxScene::Create(lSdkManager, "Cone Up");
    LoadScene(lSdkManager, lConeUp, (assetPathP / "cone_up.fbx").string());
    FbxNode* lConeUpNode = lConeUp->GetRootNode()->GetChild(0);
    FbxObject* lConeUpObject = lConeUpNode->GetNodeAttribute();

    FbxScene* lConeLying = FbxScene::Create(lSdkManager, "Cone Lying");
    LoadScene(lSdkManager, lConeLying, (assetPathP / "cone_lying.fbx").string());
    FbxNode* lConeLyingNode = lConeLying->GetRootNode()->GetChild(0);
    FbxObject* lConeLyingObject = lConeLyingNode->GetNodeAttribute();

    FbxScene* lPointCube = FbxScene::Create(lSdkManager, "Point Cube");
    LoadScene(lSdkManager, lPointCube, (assetPathP / "point_cube.fbx").string());
    FbxNode* lPointCubeNode = lPointCube->GetRootNode()->GetChild(0);
    FbxObject* lPointCubeObject = lPointCubeNode->GetNodeAttribute();

    // Node array containing all children to populate into new scene
    std::vector<FbxNode*> lChildren;

    // Add road and wall to scene
    FbxNode* groundNode = lGround->GetRootNode()->GetChild(0);
    groundNode->SetName("1ROAD");
    lChildren.push_back(groundNode);
    lGround->GetRootNode()->DisconnectAllSrcObject();
    int lNumSceneObjects = lGround->GetSrcObjectCount();
    for (int i = 0; i < lNumSceneObjects; i++) 
    {
        FbxObject* lObj = lGround->GetSrcObject(i);
        if (lObj == lGround->GetRootNode() || *lObj == lGround->GetGlobalSettings()) 
        {
            continue;
        }
        // Attach object to the reference scene.
        lObj->ConnectDstObject(lFinalScene);
    }
    lGround->DisconnectAllSrcObject();

    FbxNode* wallNode = lWall->GetRootNode()->GetChild(0);
    wallNode->SetName("1WALL");
    lChildren.push_back(wallNode);
    lWall->GetRootNode()->DisconnectAllSrcObject();
    lNumSceneObjects = lWall->GetSrcObjectCount();
    for (int i = 0; i < lNumSceneObjects; i++) 
    {
        FbxObject* lObj = lWall->GetSrcObject(i);
        if (lObj == lWall->GetRootNode() || *lObj == lWall->GetGlobalSettings()) 
        {
            continue;
        }
        // Attach object to the reference scene.
        lObj->ConnectDstObject(lFinalScene);
    }
    lWall->DisconnectAllSrcObject();


    // Texture and material for cloned objects
    std::string texpath = (assetPathP / "diffuse.png").string();
    FbxString lMaterialName = "material";
    FbxDouble3 lDiffuseColor(1.0, 0.25, 0.0);
    FbxSurfacePhong* gMaterial = FbxSurfacePhong::Create(lSdkManager, lMaterialName.Buffer());
    FbxFileTexture* lTexture = FbxFileTexture::Create(lFinalScene, "Diffuse Texture");
    lTexture->SetFileName(texpath.c_str()); // Resource file is in current directory.
    lTexture->SetTextureUse(FbxTexture::eStandard);
    lTexture->SetMappingType(FbxTexture::eUV);
    lTexture->SetMaterialUse(FbxFileTexture::eModelMaterial);
    lTexture->SetSwapUV(false);
    lTexture->SetTranslation(0.0, 0.0);
    lTexture->SetScale(1.0, 1.0);
    lTexture->SetRotation(0.0, 0.0);
    gMaterial->Diffuse = lDiffuseColor;
    gMaterial->Shininess = 0.5;
    gMaterial->Diffuse.ConnectSrcObject(lTexture);


    // ---------------------------------------------------
    // All cones
    // ---------------------------------------------------
    int coneCounter = 0;
    for (int i = 0; i < points.size(); i++) 
    {

        FbxObject* lCloneConeObject;
        FbxNode* lCloneConeNode;
        FbxObject* lCloneConeExtraObject = NULL;
        FbxNode* lCloneConeExtraNode = NULL;
        std::stringstream ss;
        std::stringstream ssExtra;
        switch (points[i].cone_id)
        {
            case 1:
                lCloneConeObject = lConeUpObject->Clone();
                lCloneConeNode = static_cast<FbxNode*>(lConeUpNode->Clone());
                lCloneConeNode->AddMaterial(gMaterial);
                ss << "AC_POBJECT_cone" << coneCounter++;
                break;
            case 2:
                lCloneConeObject = lConeUpObject->Clone();
                lCloneConeNode = static_cast<FbxNode*>(lConeUpNode->Clone());
                lCloneConeNode->AddMaterial(gMaterial);
                ss << "AC_POBJECT_cone" << coneCounter++;
                lCloneConeExtraObject = lConeLyingObject->Clone();
                lCloneConeExtraNode = static_cast<FbxNode*>(lConeLyingNode->Clone());
                lCloneConeExtraNode->AddMaterial(gMaterial);
                ssExtra << "AC_POBJECT_cone" << coneCounter++;
                break;
            case 3:
                lCloneConeObject = lConeLyingObject->Clone();
                lCloneConeNode = static_cast<FbxNode*>(lConeLyingNode->Clone());
                lCloneConeNode->AddMaterial(gMaterial);
                ss << "AC_POBJECT_cone" << coneCounter++;
                break;
            case 4:
                lCloneConeObject = lPointCubeObject->Clone();
                lCloneConeNode = static_cast<FbxNode*>(lPointCubeNode->Clone());
                lCloneConeNode->AddMaterial(gMaterial);
                ss << "AC_PIT_0";
                lCloneConeExtraObject = lCloneConeObject->Clone();
                lCloneConeExtraNode = static_cast<FbxNode*>(lCloneConeNode->Clone());
                lCloneConeExtraNode->AddMaterial(gMaterial);
                ssExtra << "AC_HOTLAP_START_0";
                break;
            default:
                continue;
        }
        lCloneConeNode->LclTranslation.Set(FbxDouble3(points[i].fbx_y, 0.225, points[i].fbx_x));
        lCloneConeNode->LclRotation.Set(FbxDouble3(0, -points[i].rotation, 0));
        lCloneConeNode->SetNodeAttribute(static_cast<FbxNodeAttribute*>(lCloneConeObject));
        lCloneConeNode->SetName(ss.str().c_str());

        lChildren.push_back(lCloneConeNode);

        if (lCloneConeExtraNode != NULL)
        {
            lCloneConeExtraNode->LclTranslation.Set(FbxDouble3(points[i].fbx_y, 0.225, points[i].fbx_x));
            lCloneConeExtraNode->LclRotation.Set(FbxDouble3(0, -points[i].rotation, 0));
            lCloneConeExtraNode->SetNodeAttribute(static_cast<FbxNodeAttribute*>(lCloneConeExtraObject));
            lCloneConeExtraNode->SetName(ssExtra.str().c_str());
            lChildren.push_back(lCloneConeExtraNode);
        }
    }

    // ---------------------------------------------------
    // Starting line
    // ---------------------------------------------------
    FbxObject* lCloneStartLeftObj = lPointCubeObject->Clone();
    FbxNode* lCloneStartLeftObjNode = static_cast<FbxNode*>(lPointCubeNode->Clone());
    lCloneStartLeftObjNode->LclTranslation.Set(FbxDouble3(startLine.fbx_y1, 0.25, startLine.fbx_x1));
    lCloneStartLeftObjNode->LclRotation.Set(FbxDouble3(0, startLine.angle, 0));
    lCloneStartLeftObjNode->AddMaterial(gMaterial);
    lCloneStartLeftObjNode->SetNodeAttribute(static_cast<FbxNodeAttribute*>(lCloneStartLeftObj));
    lCloneStartLeftObjNode->SetName("AC_AB_START_L");
    lChildren.push_back(lCloneStartLeftObjNode);

    FbxObject* lCloneStartRightObj = lPointCubeObject->Clone();
    FbxNode* lCloneStartRightObjNode = static_cast<FbxNode*>(lPointCubeNode->Clone());
    lCloneStartRightObjNode->LclTranslation.Set(FbxDouble3(startLine.fbx_y2, 0.25, startLine.fbx_x2));
    lCloneStartRightObjNode->LclRotation.Set(FbxDouble3(0, startLine.angle, 0));
    lCloneStartRightObjNode->AddMaterial(gMaterial);
    lCloneStartRightObjNode->SetNodeAttribute(static_cast<FbxNodeAttribute*>(lCloneStartRightObj));
    lCloneStartRightObjNode->SetName("AC_AB_START_R");
    lChildren.push_back(lCloneStartRightObjNode);


    // ---------------------------------------------------
    // Finish line
    // ---------------------------------------------------
    FbxObject* lCloneFinishLeftObj = lPointCubeObject->Clone();
    FbxNode* lCloneFinishLeftObjNode = static_cast<FbxNode*>(lPointCubeNode->Clone());
    lCloneFinishLeftObjNode->LclTranslation.Set(FbxDouble3(finishLine.fbx_y1, 0.25, finishLine.fbx_x1));
    lCloneFinishLeftObjNode->LclRotation.Set(FbxDouble3(0, finishLine.angle, 0));
    lCloneFinishLeftObjNode->AddMaterial(gMaterial);
    lCloneFinishLeftObjNode->SetNodeAttribute(static_cast<FbxNodeAttribute*>(lCloneFinishLeftObj));
    lCloneFinishLeftObjNode->SetName("AC_AB_FINISH_L");
    lChildren.push_back(lCloneFinishLeftObjNode);

    FbxObject* lCloneFinishRightObj = lPointCubeObject->Clone();
    FbxNode* lCloneFinishRightObjNode = static_cast<FbxNode*>(lPointCubeNode->Clone());
    lCloneFinishRightObjNode->LclTranslation.Set(FbxDouble3(finishLine.fbx_y2, 0.25, finishLine.fbx_x2));
    lCloneFinishRightObjNode->LclRotation.Set(FbxDouble3(0, finishLine.angle, 0));
    lCloneFinishRightObjNode->AddMaterial(gMaterial);
    lCloneFinishRightObjNode->SetNodeAttribute(static_cast<FbxNodeAttribute*>(lCloneFinishRightObj));
    lCloneFinishRightObjNode->SetName("AC_AB_FINISH_R");
    lChildren.push_back(lCloneFinishRightObjNode);


    // Add all children to scene
    for (int i = 0; i < lChildren.size(); i++)
    {
        lFinalScene->GetRootNode()->AddChild(lChildren[i]);
    }

    // Prepare export
    lSdkManager->GetIOSettings()->SetBoolProp(EXP_FBX_EMBEDDED, true);
    FbxExporter* pExporter = FbxExporter::Create(lSdkManager, "");
    pExporter->SetFileExportVersion(FBX_2013_00_COMPATIBLE);

    std::vector<char> buffer;
    MemoryStream* stream = new MemoryStream(buffer);
    bool pExportStatus = pExporter->Initialize(stream);

    if (!pExportStatus) 
    {
        printf("Call to FbxExporter::Initialize() failed.\n");
        printf("Error returned: %s\n\n", pExporter->GetStatus().GetErrorString());
        throw;
    }
    pExporter->Export(lFinalScene);


    // clear memory
    lTexture->Destroy(true);
    gMaterial->Destroy(true);
    lGround->Destroy(true);
    lWall->Destroy(true);
    lConeUp->Destroy(true);
    lConeLying->Destroy(true);
    lPointCube->Destroy(true);
    pExporter->Destroy(true);
    lFinalScene->Destroy(true);
    lSdkManager->Destroy();

    return stream;
}
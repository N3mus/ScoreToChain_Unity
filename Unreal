// ScoreUploader.h

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "ScoreUploader.generated.h"

UCLASS()
class YOURGAME_API AScoreUploader : public AActor
{
    GENERATED_BODY()

public:
    AScoreUploader();

    UFUNCTION(BlueprintCallable, Category = "Score Upload")
    void SubmitScore(const FString& WalletAddress, float Score, const TMap<FString, int32>& AdditionalData);

private:
    void OnResponseReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bWasSuccessful);
};


// ScoreUploader.cpp

#include "ScoreUploader.h"
#include "HttpModule.h"
#include "Interfaces/IHttpRequest.h"
#include "Interfaces/IHttpResponse.h"
#include "Json.h"
#include "JsonUtilities.h"

AScoreUploader::AScoreUploader()
{
    PrimaryActorTick.bCanEverTick = false;
}

void AScoreUploader::SubmitScore(const FString& WalletAddress, float Score, const TMap<FString, int32>& AdditionalData)
{
    // Prepare URL
    FString StudioApiUrl = TEXT("https://studio-backend.com/postMatchResults");

    // Create JSON object
    TSharedPtr<FJsonObject> JsonObject = MakeShareable(new FJsonObject);
    JsonObject->SetStringField("address", WalletAddress);
    JsonObject->SetStringField("amount", FString::SanitizeFloat(Score)); // Ensure score is sent as a string
     
    // Convert AdditionalData keys and values into JSON arrays
    TArray<TSharedPtr<FJsonValue>> JsonKeys;
    TArray<TSharedPtr<FJsonValue>> JsonValues;

    for (const TPair<FString, int32>& Pair : AdditionalData)
    {
        JsonKeys.Add(MakeShareable(new FJsonValueString(Pair.Key)));
        JsonValues.Add(MakeShareable(new FJsonValueNumber(Pair.Value)));
    }

    JsonObject->SetArrayField("keys", JsonKeys);
    JsonObject->SetArrayField("values", JsonValues);

    // Convert JSON object to string
    FString JsonOutput;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&JsonOutput);
    FJsonSerializer::Serialize(JsonObject.ToSharedRef(), Writer);

    // Create HTTP request
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> Request = FHttpModule::Get().CreateRequest();
    Request->SetURL(StudioApiUrl);
    Request->SetVerb("POST");
    Request->SetHeader("Content-Type", "application/json");
    Request->SetContentAsString(JsonOutput);
    Request->OnProcessRequestComplete().BindUObject(this, &AScoreUploader::OnResponseReceived);
    Request->ProcessRequest();
}

void AScoreUploader::OnResponseReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bWasSuccessful)
{
    if (bWasSuccessful && Response.IsValid())
    {
        UE_LOG(LogTemp, Log, TEXT("✅ Score successfully submitted: %s"), *Response->GetContentAsString());
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("❌ Failed to submit score!"));
        if (Response.IsValid())
        {
            UE_LOG(LogTemp, Error, TEXT("⚠️ Server response: %s"), *Response->GetContentAsString());
        }
    }
}




🚀 How to Use in Unreal
Attach ScoreUploader to an Actor in your Unreal scene.
Call SubmitScore() from Blueprints or C++:

AScoreUploader* Uploader = GetWorld()->SpawnActor<AScoreUploader>();
TMap<FString, int32> AdditionalData;
AdditionalData.Add("gameMode", 1);
AdditionalData.Add("difficulty", 3);
Uploader->SubmitScore("0x1234567890abcdef1234567890abcdef12345678", 10.0f, AdditionalData);


Check Output Log (Window > Developer Tools > Output Log) for debug messages.



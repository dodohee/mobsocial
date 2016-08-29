﻿"use strict";

window.attachFunctionToTimelineScope = function(name, func) {
    //to make things pluggable, we have created a function so that other plugin vendors can inject their functions to timeline without 
    //having to modify timeline.js core file

    var $scope = angular.element(".activity-list").scope();

    //in this scope variable, set the function
    $scope[name] = func;
    return $scope;
}

window.mobSocial.lazy.controller("timelineController", [
    '$scope', '$sce', 'timelineService', function ($scope, $sce, TimelineService) {

      
        $scope.ClearPostFormExtraData = function () {
            $scope.PostData.AdditionalAttributeValue = null;
            $scope.PostData.PostTypeName = "status";
        }

        $scope.PostToTimeline = function () {
            if ($scope.PostData.Message == "" && $scope.PostData.AdditionalAttributeValue == "") {
                //nothing to post, just return
                return;
            }
            TimelineService.PostToTimeline($scope.PostData, function (response) {
                if (response.Success)
                    $scope.TimelinePosts.unshift(response.Post); //prepend to existing list
                //clear data
                $scope.PostData = {
                    Message: "",
                    PostTypeName: "status",
                    AdditionalAttributeValue: null

                };
            }, function() {
                alert("Error occured while processing the request");
            });
        };

        $scope.GetTimelinePosts = function() {
            //next page
            $scope.timelinePostsRequestModel.page++;
            
            TimelineService.GetTimelinePosts($scope.timelinePostsRequestModel, function (response) {
                if (response.length == 0) {
                    $scope.NoMorePosts = true;
                    return;
                }
                //append to existing list
                $scope.TimelinePosts = $scope.TimelinePosts.concat(response);
            }, function() {
                alert("An error occured while processing request");
            });
        }

        $scope.DeletePost = function (postId) {
            if (!confirm("Delete this post?")) {
                return;
            }
            TimelineService.DeletePost(postId, function (response) {
                if (response.Success) {
                    for (var i = 0; i < $scope.TimelinePosts.length; i++) {
                        var post = $scope.TimelinePosts[i];
                        if (post.Id == postId) {
                            $scope.TimelinePosts.splice(i, 1);
                            break;
                        }
                    }
                }
            }, function(response) {
                alert("An error occured while processing request");
            });
        }
      
        //ctor
        $scope.init = function() {
            $scope.timelinePostsRequestModel = {
                page: 0,
                count: 15
            };
            $scope.PostData = {
                Message: "",
                PostTypeName: "status",
                AdditionalAttributeValue: null
            };

            //the posts being shown
            $scope.TimelinePosts = [];

            //the preview data for a post
            $scope.PostPreview = {};

            //request posts so far
            $scope.GetTimelinePosts();

            //functions to execute before selecting a post to render
            //plugin vendors can write their own functions to be executed before showing the post.
            //this will help to deserialize the additionalattributevalue to be displayed in the post display view
            $scope.FilterFunction = [];
        }();

    }
]);
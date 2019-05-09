#!/usr/bin/env python
# -*- coding: utf-8 -*-
import rospy
import math
import json
import os
from subprocess import call
from enum import Enum #sudo pip install enum34
from ruamel import yaml # sudo pip install ruamel.yaml
from std_msgs.msg import String, Bool
from std_msgs.msg import Int16,Int32,Int64, Float32, Float64, UInt32
from geometry_msgs.msg import PoseStamped,PoseWithCovarianceStamped,Vector3
from actionlib_msgs.msg import GoalStatusArray
from nav_msgs.msg import Odometry
from time import sleep

class ResponseStatus(Enum):
	RESPONSE_NONE = 0;
    	RESPONSE_LASER_CAME_POINT = 2000;
	RESPONSE_LINEDETECT_PALLETUP = 3203;
    	RESPONSE_LINEDETECT_PALLETDOWN = 3204;
    	RESPONSE_FINISH_GOTO_POSITION = 3205;
    	RESPONSE_FINISH_DETECTLINE_CHARGEAREA = 3206;
    	RESPONSE_FINISH_RETURN_LINE_CHARGEAREA = 3207;
    	RESPONSE_FINISH_TURN_LEFT = 3210;
    	RESPONSE_FINISH_TURN_RIGHT = 3211;
    	RESPONSE_FINISH_GOBACK_FRONTLINE = 3213;
    	RESPONSE_ERROR = 3215;
	RESPONSE_POS_PALLET=3216;
	RESPONSE_AREA_PALLET = 3217;
	RESPONSE_ROBOT_NAVIGATION = 3218;
	RESPONSE_LINE_CTRL = 3219;
class CallErrorLineDetect(Enum):
	CallERROR_LINEDETECTION=4205;
class SelfDriving():
	def __init__(self):
    #####################################################
		print("Initializing SelfDriving Class...")
        	rospy.init_node("SelfDriving")
        	self.nodename = rospy.get_name()
        	print("%s started" % self.nodename)
		self.rate=rospy.get_param('~rate',50);
		self.amclpose_posX=0;
		self.amclpose_posY=0;
		self.amclpose_posthetaW=0.0;
		self.amclpose_posthetaZ=0.0;
		self.currentgoal_x=0.0;
		self.currentgoal_y=0.0;
		self.currentgoal_z=0.0;
		self.currentgoal_w=0.0;
		self.current_Vx=0.0;
		self.current_Vy=0.0;
		self.current_W=0.0;
		self.current_W=0.0;
		self.errorVx=0.0001;
		self.errorVy=0.0001;
		self.errorW=0.0001;
		self.errorDx=0.5;
		self.errorDy=0.5;
		self.cntGoal=0;
		self.fnewGoal=False;
		self.cntTimeOutRequest=0;
		self.amoutOfErrorInLineDetection=3; # 3 times errors
		self.amoutOfRefreshLineDetection=10; # 3 times errors
		self.flagReachedGoal=False;
		self.pub=rospy.Publisher('chatter', String, queue_size=10)
		self.pub_navigation_setgoal=rospy.Publisher('/move_base_simple/goal',PoseStamped,queue_size=100);
		self.pub_FinishedStates=rospy.Publisher('finishedStates', Int32, queue_size=100);

		rospy.Subscriber('/robot_navigation',PoseStamped,self.moveBaseSimple_goal,queue_size=100);
		#rospy.Subscriber('battery_vol',UInt32,self.batterysub_callback,queue_size=100);
		rospy.Subscriber('linedetectioncallback',Int32,self.LineDetection_callback,queue_size=1);
		rospy.Subscriber('amcl_pose',PoseWithCovarianceStamped,self.navigationAmclPose_callback,queue_size=100);
		rospy.Subscriber('odom',Odometry,self.odometry_callback,queue_size=100);
		rospy.Subscriber('move_base/status',GoalStatusArray,self.reachedGoal_Callback,queue_size=100);
		rospy.Subscriber('goalConfirm',Vector3,self.goalConfirmCallBack,queue_size=10)

		self.pub_respCtrl=rospy.Publisher('respCtrl', Int32, queue_size=100);
		rospy.Subscriber('linedetectionctrl_servercallback',Int32,self.Linedetectionctrl_Servercallback,queue_size=1);
		rospy.Subscriber('pospallet_servercallback',Int32,self.Pospallet_Servercallback,queue_size=1);
		rospy.Subscriber('cmdAreaPallet_servercallback',String,self.CmdAreaPallet_Servercallback,queue_size=1);
		
		self.pub_linedetectionctrl=rospy.Publisher('linedetectionctrl', Int32, queue_size=10)
		self.pub_pospallet=rospy.Publisher('pospallet', Int32, queue_size=10)
		self.pub_cmdAreaPallet=rospy.Publisher('cmdAreaPallet', String, queue_size=10)
	
	def spin(self):
	        self.r = rospy.Rate(self.rate)
		while not rospy.is_shutdown():
			self.r.sleep()
	def counterTimeOutRequest(self,second):
		if self.cntTimeOutRequest>=second:
			self.cntTimeOutRequest=0;
			flagSetTimeOut=True;
		else:
			self.cntTimeOutRequest+=1;
			flagSetTimeOut=False;
			sleep(1);
		return flagSetTimeOut
	def navigationAmclPose_callback(self,msg):
		self.amclpose_posX=msg.pose.pose.position.x;
		self.amclpose_posY=msg.pose.pose.position.y;
		self.amclpose_posthetaW=msg.pose.pose.orientation.w;
		self.amclpose_posthetaZ=msg.pose.pose.orientation.z;
	def odometry_callback(self,odom):
		self.current_Vx=odom.twist.twist.linear.x;
		self.current_Vy =odom.twist.twist.linear.y;
		self.current_W  =odom.twist.twist.angular.z;
		#print("PROCESS_SELFDRIVING_DETECTLINE_TO_CHARGEAREA");
	def posPalletCtrl(self,cmd):
		print(cmd)
		numctrl=cmd.value
		self.pub_posPallet.publish(numctrl);
	def goalConfirmCallBack(self,msg):
		try:
# 			print(msg)
			self.currentgoal_x = msg.x
			self.currentgoal_y = msg.y		
			self.fnewGoal = True
			self.pub_respCtrl.publish(ResponseStatus.RESPONSE_ROBOT_NAVIGATION.value);
	# 		self.dataGoalConfirm = self.Point(msg.x,msg.y,msg.z).__dict__
	# 		self.fgoalConfirm = True
		except:
			print("goalConfirmCallBack fail")
	def linedetectionctrl(self,cmd):
		print(cmd)
		numctrl=cmd.value
		self.pub_linedetectionctrl.publish(numctrl);
	def pubFinishedStates(self,state):
		self.pub_FinishedStates.publish(state);
	def moveBaseSimple_goal(self,pose):
		#self.currentgoal_x=pose.pose.position.x;
		#self.currentgoal_y=pose.pose.position.y;
		#self.currentgoal_z=pose.pose.orientation.z;
		#self.currentgoal_w=pose.pose.orientation.w;
		#print(pose);
		self.pub_navigation_setgoal.publish(pose);
	def Linedetectionctrl_Servercallback(self,msg):
		self.pub_linedetectionctrl.publish(msg);
		value=ResponseStatus.RESPONSE_LINE_CTRL.value
		self.pub_respCtrl.publish(value);
	def Pospallet_Servercallback(self,msg):
		self.pub_pospallet.publish(msg);
                value=ResponseStatus.RESPONSE_POS_PALLET.value
#		print(value)
		self.pub_respCtrl.publish(value);
	def CmdAreaPallet_Servercallback(self,msg):
		self.pub_cmdAreaPallet.publish(msg);
                value=ResponseStatus.RESPONSE_AREA_PALLET.value
		self.pub_respCtrl.publish(ResponseStatus.RESPONSE_AREA_PALLET.value);
		

	def LineDetection_callback(self,msg):
		self.pubFinishedStates(msg.data);
	def reachedGoal_Callback(self,msg):
		if len(msg.status_list):
			if(self.fnewGoal == True):
				if msg.status_list[0].status==msg.status_list[0].SUCCEEDED:
					_currentgoal_Ex = math.fabs(self.amclpose_posX-self.currentgoal_x)
					_currentgoal_Ey = math.fabs(self.amclpose_posY-self.currentgoal_y)
					if (_currentgoal_Ex <= self.errorDx and _currentgoal_Ey <= self.errorDy):
						if (math.fabs(self.current_Vx) < self.errorVx):		
							self.pubFinishedStates(ResponseStatus.RESPONSE_LASER_CAME_POINT.value);
							self.fnewGoal = False			
							print("REACHED GOAL")						

if __name__ == '__main__':
    try:
	#call("rosrun seldat_robot reachGoal&", shell=True)  
    	selfDriving = SelfDriving()
	selfDriving.spin()
    except rospy.ROSInterruptException:
        pass

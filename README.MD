# 画饼中……

# 开发
## 开发注意点
- 尽量使用`object`代替`dynamic`。因为单元测试中使用了Microsoft.Fakes，而Microsoft.Fakes对`dynamic`支持很差。只在确实要调用不确定类型的实例成员函数、字段的情况下使用`dynamic`。也不可因噎废食，用反射调用成员替代`dynamic`。
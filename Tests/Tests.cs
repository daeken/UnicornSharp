using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UnicornSharp;

namespace Tests {
	[TestFixture]
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	public class Tests {
		[Test]
		public void OpenArm() {
			Assert.DoesNotThrow(() => new UnicornArm());
		}

		[Test]
		public void ArmRegReadWrite() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc[ArmRegister.R0] = 0xdeadbeef);
			ulong value = 0;
			Assert.DoesNotThrow(() => value = uc[ArmRegister.R0]);
			Assert.AreEqual(0xdeadbeef, value);
		}

		[Test]
		public void MemMap() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc.Map(0, 0x1000, MemoryPermission.All));
		}
		
		[Test]
		public void MemReadWrite() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc.Map(0, 0x1000, MemoryPermission.All));
			Assert.DoesNotThrow(() => uc.MemWrite(0, new byte[] { 0xde, 0xad, 0xbe, 0xef }));
			byte[] temp = null;
			Assert.DoesNotThrow(() => temp = uc.MemRead(0, 4));
			Assert.AreEqual(0xde, temp[0]);
			Assert.AreEqual(0xad, temp[1]);
			Assert.AreEqual(0xbe, temp[2]);
			Assert.AreEqual(0xef, temp[3]);
		}
		
		[Test]
		public void MemReadWriteNumber() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc.Map(0, 0x1000, MemoryPermission.All));
			Assert.DoesNotThrow(() => uc.MemWrite(0, new byte[] { 0xbe, 0xba, 0xfe, 0xca, 0xef, 0xbe, 0xad, 0xde }));
			Assert.DoesNotThrow(() => {
				uc.MemRead(0, out byte val);
				Assert.AreEqual(0xbe, val);
			});
			Assert.DoesNotThrow(() => {
				uc.MemRead(0, out ushort val);
				Assert.AreEqual(0xbabe, val);
			});
			Assert.DoesNotThrow(() => {
				uc.MemRead(0, out uint val);
				Assert.AreEqual(0xcafebabe, val);
			});
			Assert.DoesNotThrow(() => {
				uc.MemRead(4, out uint val);
				Assert.AreEqual(0xdeadbeef, val);
			});
			Assert.DoesNotThrow(() => {
				uc.MemRead(0, out ulong val);
				Assert.AreEqual(0xdeadbeefcafebabe, val);
			});
		}

		[Test]
		public void ArmArithmetic() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc.Map(0xf000, 0x1000, MemoryPermission.All));
			Assert.DoesNotThrow(() => uc.MemWrite(0xf000, new byte[] { 0x02, 0x00, 0x81, 0xe0 }));
			uc[ArmRegister.R0] = 0;
			uc[ArmRegister.R1] = 125;
			uc[ArmRegister.R2] = 908;
			Assert.DoesNotThrow(() => uc.Start(0xf000, 0xf004, count: 1));
			Assert.AreEqual(125, uc[ArmRegister.R1]);
			Assert.AreEqual(908, uc[ArmRegister.R2]);
			Assert.AreEqual(125 + 908, uc[ArmRegister.R0]);
		}

		[Test]
		public void ArmSvc() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc.Map(0xf000, 0x1000, MemoryPermission.All));
			Assert.DoesNotThrow(() => uc.MemWrite(0xf000, new byte[] { 0xf0, 0x00, 0x00, 0xef }));
			var inthit = 0xFFFFFFFFU;
			uc.OnInterrupt += (_, intno) => inthit = intno;
			Assert.DoesNotThrow(() => uc.Start(0xf000, 0xf004, count: 1));
			Assert.AreEqual(2, inthit);
		}

		[Test]
		public void ArmSvcUnhandled() {
			UnicornArm uc = null;
			Assert.DoesNotThrow(() => uc = new UnicornArm());
			Assert.DoesNotThrow(() => uc.Map(0xf000, 0x1000, MemoryPermission.All));
			Assert.DoesNotThrow(() => uc.MemWrite(0xf000, new byte[] { 0xf0, 0x00, 0x00, 0xef }));
			Assert.Throws<UnicornException>(() => uc.Start(0xf000, 0xf004, count: 1));
		}
	}
}